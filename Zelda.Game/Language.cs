using System;

namespace Zelda.Game
{
    static class Language
    {
        public static string LanguageCode { get; private set; }

        public static void SetLanguage(string languageCode)
        {
            Debug.CheckAssertion(HasLanguage(languageCode), "No such language: '{0}'".F(languageCode));

            LanguageCode = languageCode;
            DialogResource.Initialize();
            StringResource.Initialize();
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
