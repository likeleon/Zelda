using System;
using System.IO;

namespace Zelda.Game
{
    public static class Debug
    {
        public static bool DieOnError { get; set; }
        
        private static Lazy<StreamWriter> _error_file = new Lazy<StreamWriter>(() => File.CreateText("error.log"));
        private static StreamWriter ErrorFile { get { return _error_file.Value; } }

        static Debug()
        {
            DieOnError = false;
        }

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
            throw new Exception(message);
        }
    }
}
