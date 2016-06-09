using SDL2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Zelda.Game.Entities;

namespace Zelda.Game
{
    public static class Exts
    {
        public static string F(this string fmt, params object[] args)
        {
            return string.Format(fmt, args);
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static IEnumerable<string> GetNamespaces(this Assembly a)
        {
            return a.GetTypes().Select(t => t.Namespace).Distinct().Where(n => n != null);
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

        public static T XmlDeserialize<T>(this byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
                return stream.XmlDeserialize<T>();
        }

        public static void XmlSerialize(this object obj, Stream stream)
        {
            var serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(stream, obj);
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

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }

        public static void Do<T>(this IEnumerable<T> e, Action<T> action)
        {
            foreach (var ee in e)
                action(ee);
        }

        public static bool TryFirstOrDefault<T>(this IEnumerable<T> source, out T found)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    found = iterator.Current;
                    return true;
                }
                else
                {
                    found = default(T);
                    return false;
                }
            }
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<T> WithoutLast<T>(this IEnumerable<T> source)
        {
            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    for (var value = e.Current; e.MoveNext(); value = e.Current)
                        yield return value;
                }
            }
        }

        public static string JoinWith(this IEnumerable<string> e, string separator)
        {
            return string.Join(separator, e);
        }

        public static void Times(this int count, Action action)
        {
            for (var i = 0; i < count; ++i)
                action();
        }

        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return dict as IReadOnlyDictionary<TKey, TValue> ?? new ReadOnlyDictionary<TKey, TValue>(dict);
        }
    }
}
