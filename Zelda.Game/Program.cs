﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Zelda.Game
{
    static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
#if DEBUG
            Debug.DieOnError = true;
#endif

            if (Debugger.IsAttached)
                return Run(args);

            AppDomain.CurrentDomain.UnhandledException += (_, e) => FatalError((Exception)e.ExceptionObject);

            try
            {
                return Run(args);
            }
            catch (Exception e)
            {
                FatalError(e);
                return -1;
            }
        }

        static int Run(string[] args)
        {
            using (var framework = new Framework(new Arguments(args)))
                framework.Run();
            return 0;
        }

        static void FatalError(Exception e)
        {
            var report = BuildExceptionReport(e).ToString();
            File.WriteAllText("Exception.log", report);
            Console.Error.WriteLine(report);
        }

        static StringBuilder BuildExceptionReport(Exception e)
        {
            return BuildExceptionReport(e, new StringBuilder(), 0);
        }

        static StringBuilder BuildExceptionReport(Exception e, StringBuilder sb, int d)
        {
            if (e == null)
                return sb;

            sb.AppendFormat("Exception of type `{0}`: {1}", e.GetType().FullName, e.Message);

            var tle = e as TypeLoadException;
            if (tle != null)
            {
                sb.AppendLine();
                Indent(sb, d);
                sb.AppendFormat("TypeName=`{0}`", tle.TypeName);
            }
            else
            {
                // TODO: more exception types
            }

            if (e.InnerException != null)
            {
                sb.AppendLine();
                Indent(sb, d); sb.Append("Inner ");
                BuildExceptionReport(e.InnerException, sb, d + 1);
            }

            sb.AppendLine();
            Indent(sb, d);
            sb.Append(e.StackTrace);

            return sb;
        }

        static void Indent(StringBuilder sb, int d)
        {
            sb.Append(new string(' ', d * 2));
        }
    }
}
