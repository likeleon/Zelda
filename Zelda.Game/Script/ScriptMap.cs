using System;
using System.Collections.Generic;
using System.IO;
using Zelda.Game.Lowlevel;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptMap : ITimerContext, IMenuContext
    {
        public ScriptGame Game { get { return Map.Game.SaveGame.ScriptGame; } }
        public ScriptHero Hero { get { return Map.Game.Hero.AsScriptEntity<ScriptHero>(); } }
        public Rectangle CameraPosition { get { return Map.CameraPosition; } }

        internal Map Map { get; set;}
    
        internal void NotifyStarted(Destination destination)
        {
            CoreToScript.Call(() => OnStarted(destination.AsScriptEntity<ScriptDestination>()));
        }

        public T GetEntity<T>(string name) where T : ScriptEntity
        {
            var entity = Map.Entities.FindEntity(name);
            if (entity == null || entity.IsBeingRemoved)
                return null;

            return entity.AsScriptEntity<T>();
        }

        protected virtual void OnStarted(ScriptDestination destination)
        {
        }

        #region 엔티티 생성
        delegate ScriptEntity CreateEntityMethod(ScriptMap map, EntityData data);

        static readonly Dictionary<EntityType, CreateEntityMethod> _entityCreationMethods
            = new Dictionary<EntityType, CreateEntityMethod>()
        {
            { EntityType.Destination, CreateDestination },
            { EntityType.Destructible, CreateDestructible },
            { EntityType.Chest, CreateChest },
            { EntityType.Npc, CreateNpc },
            { EntityType.Block, CreateBlock },
            { EntityType.DynamicTile, CreateDynamicTile }
        };

        internal static void CreateMapEntityFromData(Map map, EntityData entityData)
        {
            EntityType type = entityData.Type;
            if (type == EntityType.Tile)
            {
                CreateTile(map.ScriptMap, entityData as TileData);
            }
            else
            {
                CreateEntityMethod method = null;
                if (!_entityCreationMethods.TryGetValue(type, out method))
                    Debug.Die("Missing entry creation function for type '{0}'".F(type));

                method(map.ScriptMap, entityData);
            }
        }

        internal static void CreateTile(ScriptMap scriptMap, TileData data)
        {
            ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                TilePattern pattern = map.Tileset.GetTilePattern(data.Pattern);

                Size size = EntityCreationCheckSize(data.Width, data.Height);
                for (int y = data.XY.Y; y < data.XY.Y + size.Height; y += pattern.Height)
                {
                    for (int x = data.XY.X; x < data.XY.X + size.Width; x += pattern.Width)
                    {
                        Tile tile = new Tile(data.Layer, new Point(x, y), pattern.Size, map.Tileset, data.Pattern);
                        map.Entities.AddEntity(tile);
                    }
                }
            });
        }

        internal static ScriptDestination CreateDestination(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                DestinationData data = entityData as DestinationData;
                Destination destination = new Destination(
                    data.Name,
                    data.Layer,
                    data.XY,
                    data.Direction,
                    data.Sprite,
                    data.Default);
                map.Entities.AddEntity(destination);

                return (map.IsStarted) ? destination.AsScriptEntity<ScriptDestination>() : null;
            });
        }

        internal static ScriptDestructible CreateDestructible(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                DestructibleData data = entityData as DestructibleData;
                Destructible destructible = new Destructible(
                    data.Name,
                    data.Layer,
                    data.XY,
                    data.Sprite,
                    new Treasure(
                        map.Game,
                        data.TreasureName,
                        data.TreasureVariant,
                        data.TreasureSavegameVariable),
                    data.Ground);
                destructible.DestructionSound = data.DestructionSound;
                destructible.Weight = data.Weight;
                destructible.CanBeCut = data.CanBeCut;
                destructible.CanExplode = data.CanExplode;
                destructible.CanRegenerate = data.CanRegenerate;
                destructible.DamageOnEnemies = data.DamageOnEnemies;
                map.Entities.AddEntity(destructible);

                return (map.IsStarted) ? destructible.AsScriptEntity<ScriptDestructible>() : null;
            });
        }

        internal static ScriptChest CreateChest(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                ChestData data = entityData as ChestData;
                Zelda.Game.Game game = map.Game;
                if (data.OpeningMethod == ChestOpeningMethod.ByInteractionIfItem)
                {
                    if (!String.IsNullOrEmpty(data.OpeningCondition) ||
                        !game.Equipment.ItemExists(data.OpeningCondition))
                    {
                        string msg = "Bad field 'OpeningCondition' (no such equipement item: '{0}'".F(data.OpeningCondition);
                        throw new InvalidDataException(msg);
                    }
                    EquipmentItem item = game.Equipment.GetItem(data.OpeningCondition);
                    if (!item.IsSaved)
                    {
                        string msg = "Bad field 'OpeneingCondition' (equipment item '{0}' is not saved".F(data.OpeningCondition);
                        throw new InvalidDataException();
                    }
                }

                Chest chest = new Chest(
                    data.Name,
                    data.Layer,
                    data.XY,
                    data.Sprite,
                    new Treasure(
                        game,
                        data.TreasureName,
                        data.TreasureVariant,
                        data.TreasureSavegameVariable));
                chest.OpeningMethod = data.OpeningMethod;
                chest.OpeningCondition = data.OpeningCondition;
                chest.OpeningConditionConsumed = data.OpeningConditionConsumed;
                chest.CannotOpenDialogId = data.CannotOpenDialog;
                map.Entities.AddEntity(chest);

                return (map.IsStarted) ? chest.AsScriptEntity<ScriptChest>() : null;
            });
        }

        internal static ScriptNpc CreateNpc(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                NpcData data = entityData as NpcData;
                Npc npc = new Npc(
                    map.Game,
                    data.Name,
                    data.Layer,
                    data.XY,
                    data.Subtype,
                    data.Sprite,
                    data.Direction,
                    data.Behavior);
                map.Entities.AddEntity(npc);

                return (map.IsStarted) ? npc.AsScriptEntity<ScriptNpc>() : null;
            });
        }

        internal static ScriptBlock CreateBlock(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                BlockData data = entityData as BlockData;

                if (data.MaximumMoves < 0 || data.MaximumMoves > 2)
                    throw new InvalidDataException("Invalid MaximumMoves: {0}".F(data.MaximumMoves));

                Block block = new Block(
                    data.Name,
                    data.Layer,
                    data.XY,
                    data.Direction,
                    data.Sprite,
                    data.Pushable,
                    data.Pullable,
                    data.MaximumMoves);
                map.Entities.AddEntity(block);

                return (map.IsStarted) ? block.AsScriptEntity<ScriptBlock>() : null;
            });
        }

        internal static ScriptDynamicTile CreateDynamicTile(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                var map = scriptMap.Map;
                var data = entityData as DynamicTileData;

                var dynamicTile = new DynamicTile(
                    data.Name,
                    data.Layer,
                    data.XY,
                    EntityCreationCheckSize(data.Width, data.Height),
                    map.Tileset,
                    data.Pattern,
                    data.EnabledAtStart);
                map.Entities.AddEntity(dynamicTile);

                return (map.IsStarted) ? dynamicTile.AsScriptEntity<ScriptDynamicTile>() : null;
            });
        }

        public static ScriptBomb CreateBomb(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptToCore.Call(() =>
            {
                Map map = scriptMap.Map;
                BombData data = entityData as BombData;

                Bomb bomb = new Bomb(data.Name, data.Layer, data.XY);
                map.Entities.AddEntity(bomb);

                return (map.IsStarted) ? bomb.AsScriptEntity<ScriptBomb>() : null;
            });
        }

        private static Size EntityCreationCheckSize(int width, int height)
        {
            if (width < 0 || width % 8 != 0)
                throw new Exception("Invalid width {0}: should be a positive multiple of 8".F(width));

            if (height < 0 || height % 8 != 0)
                throw new Exception("Invalid height {0}: should be a positive multiple of 8".F(height));

            return new Size(width, height);
        }
        #endregion
    }
}
