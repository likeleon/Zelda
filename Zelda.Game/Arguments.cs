using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game
{
    // 프로그램으로 전달되는 런타임 인자들을 저장
    class Arguments
    {
        public string ProgramName { get; }
        public IEnumerable<string> Args { get; }

        public Arguments(string[] args)
        {
            ProgramName = Environment.GetCommandLineArgs()[0];
            Args = args;
        }

        public bool HasArgument(string argument)
        {
            return Args.Contains(argument);
        }

        public string GetArgumentValue(string key)
        {
            foreach (var arg in Args)
            {
                var tokens = arg.Split('=');
                if (tokens.Length >= 2 && tokens[0] == key)
                    return tokens[1];
            }
            return null;
        }
    }
}
