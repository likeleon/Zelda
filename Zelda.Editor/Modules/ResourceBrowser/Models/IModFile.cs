using System;
using System.Collections.Generic;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    interface IModFile
    {
        string Path { get; set; }
        ModFileType FileType { get; }
        IModFile Parent { get; }
        IEnumerable<IModFile> Children { get; }
        string Name { get; }
        Uri Icon { get; }
        string Description { get; set; }
        string Type { get; }
        string ToolTip { get; }
        int Depth { get; }
        int Level { get; }
        bool IsExpanded { get; set; }

        void AddChild(IModFile child);
        void RemoveChild(IModFile child);
        void RemoveFromParent();
    }
}
