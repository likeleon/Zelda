using System;
using System.Collections.Generic;

namespace Zelda.Editor.Core.Mods
{
    interface IModFile
    {
        string Path { get; }
        ModFileType FileType { get; }
        IModFile Parent { get; }
        IEnumerable<IModFile> Children { get; }
        string Name { get; }
        Uri Icon { get; }
        string Description { get; }
        string Type { get; }
        string ToolTip { get; }
        int Depth { get; }
        int Level { get; }
    }
}
