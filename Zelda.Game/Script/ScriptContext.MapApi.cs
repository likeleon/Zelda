using System;
using System.Collections.Generic;
using System.IO;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        static readonly Dictionary<EntityType, Func<Map, EntityData, MapEntity>> _entityCreationFunctions
            = new Dictionary<EntityType, Func<Map, EntityData, MapEntity>>()
        {
            { EntityType.Destination, CreateDestination },
            { EntityType.Destructible, CreateDestructible },
            { EntityType.Chest, CreateChest }
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

        // TODO: static Map.CreateTile() method
        public static void CreateTile(Map map, TileData data)
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

        public static Destination CreateDestination(Map map, EntityData entityData)
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

        public static Destructible CreateDestructible(Map map, EntityData entityData)
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

        public static Chest CreateChest(Map map, EntityData entityData)
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

        private static Size EntityCreationCheckSize(int width, int height)
        {
            if (width < 0 || width % 8 != 0)
                throw new Exception("Invalid width {0}: should be a positive multiple of 8".F(width));

            if (height < 0 || height % 8 != 0)
                throw new Exception("Invalid height {0}: should be a positive multiple of 8".F(height));

            return new Size(width, height);
        }
    }
}
