using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Zelda.Game
{
    // 프로그램으로 전달되는 런타임 인자들을 저장
    public class Arguments
    {
        public string ProgramName { private get; set; }

        private readonly Dictionary<string, string> _args = new Dictionary<string, string>();

        public Arguments(string[] args)
        {
            Regex regex = new Regex("([^=]+)=(.*)");
            foreach (string arg in args)
            {
                Match m = regex.Match(arg);
                if (!m.Success)
                    continue;

                _args[m.Groups[1].Value] = m.Groups[2].Value;
            }
        }

        public bool Contains(string key)
        {
            return _args.ContainsKey(key);
        }

        public string GetValue(string key, string defaultValue)
        {
            return Contains(key) ? _args[key] : defaultValue;
        }
    }
}
