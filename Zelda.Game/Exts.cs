using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Zelda.Game.Engine;
using Zelda.Game.Entities;

namespace Zelda.Game
{
    public static class Exts
    {
        public static string F(this string fmt, params object[] args)
        {
            return string.Format(fmt, args);
        }

        public static IEnumerable<string> GetNamespaces(this Assembly a)
        {
            return a.GetTypes().Select(t => t.Name).Distinct().Where(n => n != null);
        }

        public static bool HasAttribute<T>(this MemberInfo mi)
        {
            return (mi.GetCustomAttributes(typeof(T), true).Length != 0);
        }

        public static Lazy<T> Lazy<T>(Func<T> p)
        {
            return new Lazy<T>(p);
        }

        [CLSCompliant(false)]
        public static SDL.SDL_Surface ToSDLSurface(this IntPtr surface)
        {
            return (SDL.SDL_Surface)Marshal.PtrToStructure(surface, typeof(SDL.SDL_Surface));
        }

        [CLSCompliant(false)]
        public static SDL.SDL_PixelFormat ToSDLPixelFormat(this IntPtr pixelFormat)
        {
            return (SDL.SDL_PixelFormat)Marshal.PtrToStructure(pixelFormat, typeof(SDL.SDL_PixelFormat));
        }

        public static T XmlDeserialize<T>(this Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream);
        }

        public static string CheckField(this string value, string name)
        {
            if (value == null)
                throw new Exception("Bad field '{0}' (non-null string expected)".F(name));

            return value;
        }

        public static int CheckField(this int? value, string name)
        {
            if (!value.HasValue)
                throw new Exception("Bad field '{0}' (non-null int expected)".F(name));

            return value.Value;
        }

        public static int OptField(this int? value, int defaultValue)
        {
            if (value.HasValue)
                return value.Value;
            else
                return defaultValue;
        }

        public static string OptField(this string value, string defaultValue)
        {
            return value ?? defaultValue;
        }

        public static bool CheckField(this bool? value, string name)
        {
            if (!value.HasValue)
                throw new Exception("Bad field '{0}' (boolean expected)".F(name));

            return value.Value;
        }

        public static bool OptField(this bool? value, bool defaultValue)
        {
            if (value.HasValue)
                return value.Value;
            else
                return defaultValue;
        }

        public static Color CheckColor(this ColorXmlData value, string name)
        {
            int r = value.R.CheckField(name + ".R");
            int g = value.G.CheckField(name + ".G");
            int b = value.B.CheckField(name + ".B");
            int a = value.A.OptField(255);
            return new Color(r, g, b, a);
        }

        [CLSCompliant(false)]
        public static T CheckField<T>(this T? value, string name) where T : struct, IConvertible
        {
            Debug.CheckAssertion(typeof(T).IsEnum, "T must be an enumerated type '{0}'".F(typeof(T).Name));

            if (!value.HasValue)
                throw new Exception("Bad field '{0}' (non-null enum '{1}' expected)".F(name, typeof(T).Name));

            return value.Value;
        }

        [CLSCompliant(false)]
        public static T OptField<T>(this T? value, string name, T defaultValue) where T : struct, IConvertible
        {
            Debug.CheckAssertion(typeof(T).IsEnum, "T must be an enumerated type");

            if (value.HasValue)
                return value.Value;
            else
                return defaultValue;
        }

        public static bool IsGroundDiagonal(this Ground ground)
        {
            switch (ground)
            {
                case Ground.WallTopRight:
                case Ground.WallTopLeft:
                case Ground.WallBottomLeft:
                case Ground.WallBottomRight:
                case Ground.WallTopRightWater:
                case Ground.WallTopLeftWater:
                case Ground.WallBottomLeftWater:
                case Ground.WallBottomRightWater:
                    return true;

                case Ground.Empty:
                case Ground.Traversable:
                case Ground.Wall:
                case Ground.LowWall:
                case Ground.DeepWater:
                case Ground.ShallowWater:
                case Ground.Grass:
                case Ground.Hole:
                case Ground.Ice:
                case Ground.Ladder:
                case Ground.Prickle:
                case Ground.Lava:
                    return false;

                default:
                    return false;
            }
        }

        public static bool CanBeStoredInMapFile(this EntityType type)
        {
            switch (type)
            {
                case EntityType.Tile:
                case EntityType.Destination:
                case EntityType.Teletransporter:
                case EntityType.Pickable:
                case EntityType.Destructible:
                case EntityType.Chest:
                case EntityType.Jumper:
                case EntityType.Enemy:
                case EntityType.Npc:
                case EntityType.Block:
                case EntityType.DynamicTile:
                case EntityType.Switch:
                case EntityType.Wall:
                case EntityType.Sensor:
                case EntityType.Crystal:
                case EntityType.CrystalBlock:
                case EntityType.ShopTreasure:
                case EntityType.Stream:
                case EntityType.Door:
                case EntityType.Stairs:
                case EntityType.Separator:
                case EntityType.Custom:
                    return true;

                case EntityType.Hero:
                case EntityType.CarriedItem:
                case EntityType.Boomerang:
                case EntityType.Explosion:
                case EntityType.Arrow:
                case EntityType.Bomb:
                case EntityType.Fire:
                case EntityType.Hookshot:
                    return false;

                default:
                    return false;
            }
        }
        
        // length가 문자열보다 크면 가능한 만큼만 잘라냅니다 (c++ std::string::substr()과 동일)
        public static string SafeSubstring(this string str, int startIndex, int length)
        {
            return str.Substring(startIndex, Math.Min(str.Length - startIndex, length));
        }

        public static Direction8 GetOpposite(this Direction8 dir8)
        {
            return (Direction8)(((int)dir8 + 4) % 8);
        }

        public static Direction8 ToDirection8(this Direction4 dir4)
        {
            return (Direction8)((int)dir4 * 2);
        }

        public static bool IsHorizontal(this Direction4 dir4)
        {
            return ((int)dir4 % 2) == 0;
        }
    }
}
