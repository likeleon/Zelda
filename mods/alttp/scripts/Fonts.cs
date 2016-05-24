using System;
using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp
{
    class Font
    {
        public string Id { get; private set; }
        public int Size { get; private set; }

        public Font(string fontId, int fontSize)
        {
            Id = fontId;
            Size = fontSize;
        }
    }

    class Fonts
    {
        public static Font GetMenuFont(string language = null)
        {
            language = language ?? Core.Mod.Language;

            if (language == "ko_KR")
                return new Font("NanumBarunGothic", 12);
            else if (language == "zh_TW" || language == "zh_CN")
                return new Font("wqy-zenhei", 12);
            else
                return new Font("minecraftia", 8);
        }

        public static Font GetDialogFont(string language = null)
        {
            language = language ?? Core.Mod.Language;

            if (language == "ko_KR")
                return new Font("NanumBarunGothic", 12);
            else if (language == "zh_TW" || language == "zh_CN")
                return new Font("wqy-zenhei", 12);
            else
                return new Font("la", 11);
        }
    }
}
