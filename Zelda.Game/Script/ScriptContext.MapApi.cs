﻿using System;
using System.Collections.Generic;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        static readonly Dictionary<EntityType, Func<Map, EntityData, MapEntity>> _entityCreationFunctions
            = new Dictionary<EntityType, Func<Map, EntityData, MapEntity>>()
        {
            { EntityType.Destination, CreateDestination }
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
