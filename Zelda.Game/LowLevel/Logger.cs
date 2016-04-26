using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zelda.Game.LowLevel
{
    public static class Logger
    {
        static Lazy<TextWriter> _errorLogFile = Exts.Lazy<TextWriter>(() => File.CreateText("error.txt"));
        static TextWriter ErrorLogFile { get { return _errorLogFile.Value; } }

        public static void Print(string message)
        {
            Print(message, Console.Out);
        }

        public static void Print(string message, TextWriter tw)
        {
            tw.WriteLineAsync("[Zelda] [" + Framework.Now + "] " + message).ConfigureAwait(false);
        }

        public static void Debug(string message)
        {
            Print("Debug: " + message);
        }

        public static void Info(string message)
        {
            Print("Info: " + message);
        }

        public static void Warning(string message)
        {
            var prefixedMessage = "Warning: " + message;
            Print(prefixedMessage);
            Print(prefixedMessage, ErrorLogFile);
        }

        public static void Error(string message)
        {
            var prefixedMessage = "Error: " + message;
            Print(prefixedMessage);
            Print(prefixedMessage, ErrorLogFile);
        }

        public static void Fatal(string message)
        {
            var prefixedMessage = "Fatal: " + message;
            Print(prefixedMessage);
            Print(prefixedMessage, ErrorLogFile);
        }
    }
}
