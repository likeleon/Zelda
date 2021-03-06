﻿using SDL2;
using System;
using System.IO;

namespace Zelda.Game
{
    static class Debug
    {
        public static bool DieOnError { get; set; }
        public static bool ShowPopupOnDie { get; set; }

        readonly static Lazy<StreamWriter> _errorFile = Exts.Lazy(() => File.CreateText("Error.log"));
        static StreamWriter ErrorFile { get { return _errorFile.Value; } }

        public static void Warning(string message)
        {
            ErrorFile.WriteLine("Warning: {0}", message);
            Console.Error.WriteLine("Warning: {0}", message);
        }

        public static void Error(string message)
        {
            if (DieOnError)
                Die(message);

            ErrorFile.WriteLine("Error: {0}", message);
            Console.Error.WriteLine("Error: {0}", message);
        }

        public static void Die(string message)
        {
            ErrorFile.WriteLine("Fatal: {0}", message);
            Console.Error.WriteLine("Fatal: {0}", message);

            if (ShowPopupOnDie)
                SDL.SDL_ShowSimpleMessageBox(SDL2.SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Error", message, IntPtr.Zero);

            throw new Exception(message);
        }

        public static void CheckAssertion(bool assertion, string errorMessage)
        {
            if (!assertion)
                Die(errorMessage);
        }
    }
}
