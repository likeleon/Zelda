﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.MainMenu;

namespace Zelda.Editor.Core
{
    abstract class ModuleBase : IModule
    {
#pragma warning disable 649
        [Import]
        IMainWindow _mainWindow;

        [Import]
        IShell _shell;
#pragma warning restore 649

        protected IMainWindow MainWindow { get { return _mainWindow; } }

        protected IShell Shell { get { return _shell; } }

        protected IMenu MainMenu { get { return _shell.MainMenu; } }

        public virtual IEnumerable<ResourceDictionary> GlobalResourceDictionaries { get { yield break; } }

        public virtual IEnumerable<IDocument> DefaultDocuments { get { yield break; } }

        public virtual IEnumerable<Type> DefaultTools { get { yield break; } }

        public virtual void PreInitialize()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual void PostInitialize()
        {
        }
    }
}
