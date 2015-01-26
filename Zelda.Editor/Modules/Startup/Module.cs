﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Zelda.Editor.Core;
using Zelda.Editor.Modules.ErrorList;
using Zelda.Editor.Modules.Output;
using Zelda.Editor.Modules.Progress;

namespace Zelda.Editor.Modules.Startup
{
    [Export(typeof(IModule))]
    public class Module : ModuleBase
    {
        private readonly IOutput _output;

        public override IEnumerable<Type> DefaultTools
        {
            get 
            {
                yield return typeof(IOutput);
                yield return typeof(IErrorList);
                yield return typeof(IProgressTool); 
            }
        }

        [ImportingConstructor]
        public Module(IOutput output)
        {
            _output = output;
        }

        public override void Initialize()
        {
            MainWindow.Title = "Zelda Editor";

            Shell.StatusBar.AddItem("Ready", new GridLength(1, GridUnitType.Star));

            _output.AppendLine("Started up");
        }
    }
}
