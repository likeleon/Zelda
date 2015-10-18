using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.ToolBars
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        public override IEnumerable<ResourceDictionary> GlobalResourceDictionaries
        {
            get
            {
                yield return new ResourceDictionary
                {
                    Source = new Uri("/Zelda.Editor;component/Modules/ToolBars/Resources/Styles.xaml", UriKind.Relative)
                };
            }
        }
    }
}
