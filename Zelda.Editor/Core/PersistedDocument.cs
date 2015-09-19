using System;
using System.IO;
using System.Threading.Tasks;

namespace Zelda.Editor.Core
{
    abstract class PersistedDocument : Document, IPersistedDocument
    {
        bool _isDirty;

        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (this.SetProperty(ref _isDirty, value))
                    UpdateDisplayName();
            }
        }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public override void CanClose(Action<bool> callback)
        {
            callback(!IsDirty);
        }

        public async Task Load(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            UpdateDisplayName();

            IsDirty = false;

            await DoLoad(filePath);
        }

        protected abstract Task DoLoad(string filePath);

        void UpdateDisplayName()
        {
            DisplayName = (IsDirty) ? FileName + "*" : FileName;
        }
    }
}
