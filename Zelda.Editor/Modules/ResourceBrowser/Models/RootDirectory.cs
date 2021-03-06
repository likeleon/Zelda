﻿using System;
using Zelda.Editor.Core.Services;
using Zelda.Game;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class RootDirectory : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.RootDirectory; } }
        public override Uri Icon { get { return "icon_solarus.png".ToIconUri(); } }

        public RootDirectory(string modName)
        {
            if (modName.IsNullOrEmpty())
                throw new ArgumentNullException("modName");

            Name = modName;
        }
    }
}
