﻿using System;

namespace Zelda.Game.Script
{
    public static class ScriptLanguage
    {
        public static string LanguageCode
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<string>(() =>
                {
                    return Language.LanguageCode;
                });
            }
        }

        public static void SetLanguage(string languageCode)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                Language.SetLanguage(languageCode);
            });
        }

        public static string GetLanguageName(string languageCode)
        {
            return ScriptTools.ExceptionBoundaryHandle<string>(() =>
            {
                if (languageCode != null)
                {
                    if (!Language.HasLanguage(languageCode))
                        throw new ArgumentException("No such language: '{0}'".F(languageCode), "languageCode");

                    return Language.GetLanguageName(languageCode);
                }
                else
                {
                    if (Language.LanguageCode == null)
                        throw new ArgumentException("No language is set", "languageCode");

                    return Language.GetLanguageName(Language.LanguageCode);
                }
            });
        }
    }
}