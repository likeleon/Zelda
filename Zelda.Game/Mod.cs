using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public class Mod : IDisposable
    {
        public ModFiles ModFiles { get; }
        public ModResources Resources { get; }
        public ObjectCreator ObjectCreator { get; }
        public string Language { get; private set; }
        public IEnumerable<string> Languages => GetResources(ResourceType.Language).Select(r => r.Key);
        public IReadOnlyDictionary<string, Dialog> Dialogs => _dialogs;
        public StringResources Strings { get; } = new StringResources();
        public ModProperties Properties { get; }

        readonly Dictionary<string, Dialog> _dialogs = new Dictionary<string, Dialog>();

        public Mod(string programName, string modPath)
        {
            ModFiles = new ModFiles(this, programName, modPath);
            Resources = XmlLoader.Load<ModResources>(ModFiles, "project_db.xml");
            ObjectCreator = new ObjectCreator(this);
            Properties = XmlLoader.Load<ModProperties>(ModFiles, "mod.xml");
            ModFiles.SetModWriteDir(Properties.ModWriteDir);
        }

        public void Dispose()
        {
            ModFiles?.Dispose();
        }

        public bool ResourceExists(ResourceType resourceType, string id)
        {
            return Resources.Exists(resourceType, id);
        }

        public IReadOnlyDictionary<string, string> GetResources(ResourceType resourceType)
        {
            return Resources.ResourceMaps[resourceType];
        }

        public string GetLanguageName(string languageCode)
        {
            if (languageCode == null)
                throw new ArgumentNullException(nameof(languageCode));

            string name;
            var languages = GetResources(ResourceType.Language);
            if (!languages.TryGetValue(languageCode, out name))
                throw new ArgumentException("No such language: '{0}'".F(languageCode), nameof(languageCode));

            return name;
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
            if (!DialogExists(dialogId))
                throw new ArgumentException("No such dialog: '{0}'".F(dialogId), nameof(dialogId));

            return Dialogs[dialogId];
        }

        public bool HasLanguage(string languageCode)
        {
            return ResourceExists(ResourceType.Language, languageCode);
        }

        public void SetLanguage(string languageCode)
        {
            if (!HasLanguage(languageCode))
                throw new ArgumentException("No such language: '{0}'".F(languageCode), nameof(languageCode));

            Language = languageCode;

            Strings.Clear();
            Strings.ImportFromBuffer(ModFiles.DataFileRead("text/strings.xml", true));

            _dialogs.Clear();
            var resources = XmlLoader.Load<DialogResources>(ModFiles, "text/dialogs.xml", true);
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
