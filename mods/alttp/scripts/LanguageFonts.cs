using System;
using Zelda.Game.Script;

namespace Alttp
{
    class LanguageFonts
    {
        public static Tuple<string, int> GetMenuFont(string language = null)
        {
            language = language ?? ScriptLanguage.Language;

            if (language == "ko_KR")
                return Tuple.Create("NanumBarunGothic", 12);
            else if (language == "zh_TW" || language == "zh_CN")
                return Tuple.Create("wqy-zenhei", 12);
            else
                return Tuple.Create("minecraftia", 8);
        }

        public static Tuple<string, int> GetDialogFont(string language = null)
        {
            language = language ?? ScriptLanguage.Language;

            if (language == "ko_KR")
                return Tuple.Create("NanumBarunGothic", 12);
            else if (language == "zh_TW" || language == "zh_CN")
                return Tuple.Create("wqy-zenhei", 12);
            else
                return Tuple.Create("la", 11);
        }
    }
}
