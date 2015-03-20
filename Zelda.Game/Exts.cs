using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

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
                Debug.Error("Bad field '{0}' (non-null string expected)".F(name));

            return value;
        }

        public static int CheckField(this int? value, string name)
        {
            if (!value.HasValue)
                Debug.Error("Bad field '{0}' (non-null int expected)".F(name));

            return value.Value;
        }

        public static int OptField(this int? value, int defaultValue)
        {
            if (value.HasValue)
                return value.Value;
            else
                return defaultValue;
        }
    }
}
