using System;
using LanguageInternal = Zelda.Game.Language;

namespace Zelda.Game.Script
{
    public static class Language
    {
        public static string LanguageCode
        {
            get { return LanguageInternal.LanguageCode; }
            set { LanguageInternal.LanguageCode = value; }
        }

        public static string GetLanguageName(string languageCode)
        {
            if (languageCode != null)
            {
                if (!LanguageInternal.HasLanguage(languageCode))
                    throw new Exception("No such language: '{0}'".F(languageCode));
                
                return LanguageInternal.GetLanguageName(languageCode);
            }
            else
            {
                if (LanguageInternal.LanguageCode == null)
                    throw new Exception("No language is set");

                return LanguageInternal.GetLanguageName(LanguageInternal.LanguageCode);
            }
        }
    }
}
