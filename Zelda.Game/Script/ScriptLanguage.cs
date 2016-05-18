using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game.Script
{
    public static class ScriptLanguage
    {
        static Lazy<string[]> _languages;
        
        public static string Language { get { return MainLoop.Mod.Language; } }
        public static IEnumerable<string> Languages { get { return _languages.Value; } }

        static ScriptLanguage()
        {
            _languages = Exts.Lazy(() => MainLoop.Mod.GetResources(ResourceType.Language).Select(r => r.Key).ToArray());
        }
        
        public static void SetLanguage(string languageCode)
        {
            ScriptToCore.Call(() =>
            {
                if (!MainLoop.Mod.HasLanguage(languageCode))
                    throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                MainLoop.Mod.SetLanguage(languageCode);
            });
        }

        public static string GetLanguageName(string languageCode)
        {
            return ScriptToCore.Call(() =>
            {
                if (languageCode != null)
                {
                    if (!MainLoop.Mod.HasLanguage(languageCode))
                        throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                    return MainLoop.Mod.GetLanguageName(languageCode);
                }
                else
                {
                    if (MainLoop.Mod.Language == null)
                        throw new ArgumentException("No language is set", "languageCode");

                    return MainLoop.Mod.GetLanguageName(MainLoop.Mod.Language);
                }
            });
        }

        public static string GetString(string key)
        {
            return ScriptToCore.Call(() =>
            {
                if (!MainLoop.Mod.StringExists(key))
                    return null;

                return MainLoop.Mod.GetString(key);
            });
        }

        public static Dialog GetDialog(string dialogId)
        {
            return ScriptToCore.Call(() =>
            {
                if (!MainLoop.Mod.DialogExists(dialogId))
                    return null;

                return MainLoop.Mod.GetDialog(dialogId);
            });
        }
    }
}
