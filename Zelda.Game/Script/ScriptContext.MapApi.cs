using System;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        internal static void CreateMapEntityFromData(Map map, EntityData entityData)
        {
            switch (entityData.Type)
            {
                case EntityType.Tile:
                    CreateTile(map, entityData as TileData);
                    break;

                default:
                    Debug.Die("Missing entry creation function for type '{0}'".F(entityData.Type));
                    break;
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
