using System;
using System.Collections.Generic;
using Zelda.Editor.Core.Mods;
using Zelda.Editor.Modules.MainMenu.Models;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    interface IModFile
    {
        IMod Mod { get; }
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
        IEnumerable<CommandMenuItem> ContextMenuItems { get; }
    }
}
