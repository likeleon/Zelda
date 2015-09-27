using System;
using Zelda.Editor.Core.Services;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class UnknownFile : ModFileBase
    {
        public override ModFileType FileType { get { return ModFileType.Unknown; } }
        public override Uri Icon { get { return "icon_file_unknown.png".ToIconUri(); } }
    }
}
