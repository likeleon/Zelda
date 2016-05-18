using System;
using System.Collections.Generic;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public class CurrentMod : IDisposable
    {
        readonly Dictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();

        public ModFiles ModFiles { get; }
        public ModResources Resources { get; }
        public string Language { get; private set; }
        public IReadOnlyDictionary<string, Dialog> Dialogs => _dialogs;
        public StringResources Strings { get; } = new StringResources();
        public ModProperties Properties { get; }

        public CurrentMod(string programName, string modPath)
        {
            ModFiles = new ModFiles(this, programName, modPath);

            Resources = new ModResources();
            Resources.ImportFromModFile(ModFiles, "project_db.xml");

            Properties = new ModProperties();
            Properties.ImportFromModFile(ModFiles, "mod.xml");

            ModFiles.SetModWriteDir(Properties.ModWriteDir);
        }

        public void Dispose()
        {
            if (ModFiles != null)
                ModFiles.Dispose();
        }

        public bool ResourceExists(ResourceType resourceType, string id)
        {
            return Resources.Exists(resourceType, id);
        }

        public IReadOnlyDictionary<string, string> GetResources(ResourceType resourceType)
        {
            return Resources.GetElements(resourceType);
        }

        public string GetLanguageName(string languageCode)
        {
            var languages = GetResources(ResourceType.Language);
            if (languages.ContainsKey(languageCode))
                return languages[languageCode];
            else
                return "";
        }

        public bool StringExists(string key)
        {
            return Strings.HasString(key);
        }

        public string GetString(string key)
        {
            return Strings.GetString(key);
        }

        public bool DialogExists(string dialogId)
        {
            return _dialogs.ContainsKey(dialogId);
        }

        public Dialog GetDialog(string dialogId)
        {
            Debug.CheckAssertion(DialogExists(dialogId), "No such dialog: '{0}'".F(dialogId));
            return Dialogs[dialogId];
        }

        public bool HasLanguage(string languageCode)
        {
            return ResourceExists(ResourceType.Language, languageCode);
        }

        public void SetLanguage(string languageCode)
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
