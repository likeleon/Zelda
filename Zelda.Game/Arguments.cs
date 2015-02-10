using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zelda.Game
{
    // 프로그램으로 전달되는 런타임 인자들을 저장
    public class Arguments
    {
        private string _programName;
        public string ProgramName
        {
            get { return _programName; }
        }

        private readonly List<string> _args;
        public IEnumerable<string> Args
        {
            get { return _args; }
        }

        public Arguments(string[] args)
        {
            _programName = Environment.GetCommandLineArgs()[0];
            _args = args.ToList();
        }

        public bool HasArgument(string argument)
        {
            return _args.Contains(argument);
        }

        public string GetArgumentValue(string key)
        {
            foreach (string arg in _args)
            {
                string[] tokens = arg.Split('=');
                if (tokens.Length >= 2 && tokens[0] == key)
                    return tokens[1];
            }
            return null;
        }
    }
}
