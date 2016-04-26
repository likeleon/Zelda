using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    static class CurrentMod
    {
        static readonly Dictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();

        public static ModResources Resources { get; private set; }
        public static string Language { get; private set; }
        public static IReadOnlyDictionary<string, Dialog> Dialogs { get { return _dialogs; } }
        public static StringResources Strings { get; } = new StringResources();

        public static void Initialize()
        {
            Resources = new ModResources();
            Resources.ImportFromModFile("project_db.xml");
        }

        public static void Quit()
        {
            Resources.Clear();
            Strings.Clear();
            _dialogs.Clear();
        }

        public static bool ResourceExists(ResourceType resourceType, string id)
        {
            return Resources.Exists(resourceType, id);
        }

        public static IReadOnlyDictionary<string, string> GetResources(ResourceType resourceType)
        {
            return Resources.GetElements(resourceType);
        }

        public static string GetLanguageName(string languageCode)
        {
            var languages = GetResources(ResourceType.Language);
            if (languages.ContainsKey(languageCode))
                return languages[languageCode];
            else
                return "";
        }

        public static bool StringExists(string key)
        {
            return Strings.HasString(key);
        }

        public static string GetString(string key)
        {
            return Strings.GetString(key);
        }

        public static bool DialogExists(string dialogId)
        {
            return _dialogs.ContainsKey(dialogId);
        }

        public static Dialog GetDialog(string dialogId)
        {
            Debug.CheckAssertion(DialogExists(dialogId), "No such dialog: '{0}'".F(dialogId));
            return Dialogs[dialogId];
        }

        public static bool HasLanguage(string languageCode)
        {
            return ResourceExists(ResourceType.Language, languageCode);
        }

        public static void SetLanguage(string languageCode)
        {
            Debug.CheckAssertion(HasLanguage(languageCode), "No such language: '{0}'".F(languageCode));

            Language = languageCode;

            Strings.Clear();
            Strings.ImportFromBuffer(ModFiles.DataFileRead("text/strings.xml", true));

            var resources = new DialogResources();
            var success = resources.ImportFromBuffer(ModFiles.DataFileRead("text/dialogs.xml", true));

            _dialogs.Clear();
            if (success)
            {
                foreach (var dialogData in resources.Dialogs.Values)
                {
                    var dialog = new Dialog()
                    {
                        Id = dialogData.Id,
                        Text = dialogData.Text
                    };
                    dialogData.Properties.Do(kvp => dialog.SetProperty(kvp.Key, kvp.Value));

                    _dialogs.Add(dialog.Id, dialog);
                }
            }
        }
    }
}
