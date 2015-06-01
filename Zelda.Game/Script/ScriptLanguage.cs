using System;
using System.Collections.Generic;
using System.Linq;
using GameLanguage = Zelda.Game.Language;

namespace Zelda.Game.Script
{
    public static class ScriptLanguage
    {
        static Lazy<List<string>> _languages;
        
        public static string Language { get { return GameLanguage.LanguageCode; } }
        public static IEnumerable<string> Languages { get { return _languages.Value; } }

        static ScriptLanguage()
        {
            _languages = Exts.Lazy(() => CurrentMod.GetResources(ResourceType.Language).Select(r => r.Key).ToList());
        }
        
        public static void SetLanguage(string languageCode)
        {
            ScriptToCore.Call(() => GameLanguage.SetLanguage(languageCode));
        }

        public static string GetLanguageName(string languageCode)
        {
            return ScriptToCore.Call(() =>
            {
                if (languageCode != null)
                {
                    if (!GameLanguage.HasLanguage(languageCode))
                        throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                    return GameLanguage.GetLanguageName(languageCode);
                }
                else
                {
                    if (GameLanguage.LanguageCode == null)
                        throw new ArgumentException("No language is set", "languageCode");

                    return GameLanguage.GetLanguageName(GameLanguage.LanguageCode);
                }
            });
        }
    }
}
