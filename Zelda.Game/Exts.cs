﻿using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

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
        public static SDL.SDL_Surface GetStruct(this IntPtr surface)
        {
            return (SDL.SDL_Surface)Marshal.PtrToStructure(surface, typeof(SDL.SDL_Surface));
        }
    }
}