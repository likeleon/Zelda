﻿using System;
using System.Collections.Generic;
using System.IO;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptMap
    {
        internal Map Map { get; private set; }

        internal void NotifyStarted(Map map, Destination destionation)
        {
            Map = map;

            ScriptTools.ExceptionBoundaryHandle(() => { OnStarted(); });
        }

        public ScriptEntity GetEntity(string name)
        {
            MapEntity entity = Map.Entities.FindEntity(name);
            return (entity != null) ? entity.ScriptEntity : null;
        }

        protected virtual void OnStarted()
        {
        }

        #region 엔티티 생성
        static readonly Dictionary<EntityType, Func<Map, EntityData, MapEntity>> _entityCreationFunctions
            = new Dictionary<EntityType, Func<Map, EntityData, MapEntity>>()
        {
            { EntityType.Destination, CreateDestination },
            { EntityType.Destructible, CreateDestructible },
            { EntityType.Chest, CreateChest },
            { EntityType.Npc, CreateNpc },
            { EntityType.Block, CreateBlock }
        };

        internal static void CreateMapEntityFromData(Map map, EntityData entityData)
        {
            EntityType type = entityData.Type;
            if (type == EntityType.Tile)
            {
                CreateTile(map, entityData as TileData);
            }
            else
            {
                Func<Map, EntityData, MapEntity> function = null;
                if (!_entityCreationFunctions.TryGetValue(type, out function))
                    Debug.Die("Missing entry creation function for type '{0}'".F(type));

                function(map, entityData);
            }
        }

        internal static void CreateTile(Map map, TileData data)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
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

        internal static Destination CreateDestination(Map map, EntityData entityData)
        {
            return ScriptTools.ExceptionBoundaryHandle<Destination>(() =>
            {
                DestinationData data = entityData as DestinationData;
                Destination destination = new Destination(
                    data.Name,
                    data.Layer,
                    data.XY,
                    data.Direction,
                    data.Sprite,
                    data.Default);
                map.Entities.AddEntity(destination);

                return (map.IsStarted) ? destination : null;
            });
        }

        internal static Destructible CreateDestructible(Map map, EntityData entityData)
        {
            return ScriptTools.ExceptionBoundaryHandle<Destructible>(() =>
            {
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

                return (map.IsStarted) ? destructible : null;
            });
        }

        internal static Chest CreateChest(Map map, EntityData entityData)
        {
            return ScriptTools.ExceptionBoundaryHandle<Chest>(() =>
            {
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

                return (map.IsStarted) ? chest : null;
            });
        }

        internal static Npc CreateNpc(Map map, EntityData entityData)
        {
            return ScriptTools.ExceptionBoundaryHandle<Npc>(() =>
            {
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

                return (map.IsStarted) ? npc : null;
            });
        }

        internal static Block CreateBlock(Map map, EntityData entityData)
        {
            return ScriptTools.ExceptionBoundaryHandle<Block>(() =>
            {
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

                return (map.IsStarted) ? block : null;
            });
        }

        public static ScriptBomb CreateBomb(ScriptMap scriptMap, EntityData entityData)
        {
            return ScriptTools.ExceptionBoundaryHandle<ScriptBomb>(() =>
            {
                Map map = scriptMap.Map;
                BombData data = entityData as BombData;

                Bomb bomb = new Bomb(data.Name, data.Layer, data.XY);
                map.Entities.AddEntity(bomb);

                return (map.IsStarted) ? (bomb.ScriptEntity as ScriptBomb) : null;
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
