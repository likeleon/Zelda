using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game.Script
{
    public static class ScriptLanguage
    {
        static Lazy<string[]> _languages;
        
        public static string Language { get { return Core.Mod.Language; } }
        public static IEnumerable<string> Languages { get { return _languages.Value; } }

        static ScriptLanguage()
        {
            _languages = Exts.Lazy(() => Core.Mod.GetResources(ResourceType.Language).Select(r => r.Key).ToArray());
        }
        
        public static void SetLanguage(string languageCode)
        {
            ScriptToCore.Call(() =>
            {
                if (!Core.Mod.HasLanguage(languageCode))
                    throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                Core.Mod.SetLanguage(languageCode);
            });
        }

        public static string GetLanguageName(string languageCode)
        {
            return ScriptToCore.Call(() =>
            {
                if (languageCode != null)
                {
                    if (!Core.Mod.HasLanguage(languageCode))
                        throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                    return Core.Mod.GetLanguageName(languageCode);
                }
                else
                {
                    if (Core.Mod.Language == null)
                        throw new ArgumentException("No language is set", "languageCode");

                    return Core.Mod.GetLanguageName(Core.Mod.Language);
                }
            });
        }

        public static string GetString(string key)
        {
            return ScriptToCore.Call(() =>
            {
                if (!Core.Mod.StringExists(key))
                    return null;

                return Core.Mod.GetString(key);
            });
        }

        public static Dialog GetDialog(string dialogId)
        {
            return ScriptToCore.Call(() =>
            {
                if (!Core.Mod.DialogExists(dialogId))
                    return null;

                return Core.Mod.GetDialog(dialogId);
            });
        }
    }
}
