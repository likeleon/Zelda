using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game.Script
{
    public static class ScriptLanguage
    {
        static Lazy<string[]> _languages;
        
        public static string Language { get { return CurrentMod.Language; } }
        public static IEnumerable<string> Languages { get { return _languages.Value; } }

        static ScriptLanguage()
        {
            _languages = Exts.Lazy(() => CurrentMod.GetResources(ResourceType.Language).Select(r => r.Key).ToArray());
        }
        
        public static void SetLanguage(string languageCode)
        {
            ScriptToCore.Call(() =>
            {
                if (!CurrentMod.HasLanguage(languageCode))
                    throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                CurrentMod.SetLanguage(languageCode);
            });
        }

        public static string GetLanguageName(string languageCode)
        {
            return ScriptToCore.Call(() =>
            {
                if (languageCode != null)
                {
                    if (!CurrentMod.HasLanguage(languageCode))
                        throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                    return CurrentMod.GetLanguageName(languageCode);
                }
                else
                {
                    if (CurrentMod.Language == null)
                        throw new ArgumentException("No language is set", "languageCode");

                    return CurrentMod.GetLanguageName(CurrentMod.Language);
                }
            });
        }

        public static string GetString(string key)
        {
            return ScriptToCore.Call(() =>
            {
                if (!CurrentMod.StringExists(key))
                    return null;

                return CurrentMod.GetString(key);
            });
        }

        public static Dialog GetDialog(string dialogId)
        {
            return ScriptToCore.Call(() =>
            {
                if (!CurrentMod.DialogExists(dialogId))
                    return null;

                return CurrentMod.GetDialog(dialogId);
            });
        }
    }
}
