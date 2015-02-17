using System;

namespace Zelda.Game
{
    static class Language
    {
        static string _languageCode;
        public static string LanguageCode
        {
            get { return _languageCode; }
            set
            {
                if (!HasLanguage(value))
                    throw new Exception("No such language: '" + value + "'");

                _languageCode = value;
            }
        }

        public static bool HasLanguage(string languageCode)
        {
            return CurrentMod.ResourceExists(ResourceType.Language, languageCode);
        }

        public static string GetLanguageName(string languageCode)
        {
            var languages = CurrentMod.GetResources(ResourceType.Language);
            string name;
            if (languages.TryGetValue(languageCode, out name))
                return name;
            return string.Empty;
        }
    }
}
