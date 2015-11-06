using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    interface IResourceBrowser : ITool
    {
        bool BuildContextMenu();
        
        void Open(IModFile file);

        void OpenSelectedFile();
        void RenameSelectedFile();
        void DeleteSelectedFile();
    }
}
