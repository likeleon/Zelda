using System;
using LanguageInternal = Zelda.Game.Language;

namespace Zelda.Game.Script
{
    public static class Language
    {
        public static string LanguageCode
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<string>(() =>
                {
                    return LanguageInternal.LanguageCode;
                });
            }
            set 
            {
                ScriptTools.ExceptionBoundaryHandle(() =>
                {
                    LanguageInternal.LanguageCode = value;
                });
            }
        }

        public static string GetLanguageName(string languageCode)
        {
            return ScriptTools.ExceptionBoundaryHandle<string>(() =>
            {
                if (languageCode != null)
                {
                    if (!LanguageInternal.HasLanguage(languageCode))
                        throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                    return LanguageInternal.GetLanguageName(languageCode);
                }
                else
                {
                    if (LanguageInternal.LanguageCode == null)
                        throw new ArgumentException("No language is set", "languageCode");

                    return LanguageInternal.GetLanguageName(LanguageInternal.LanguageCode);
                }
            });
        }
    }
}
