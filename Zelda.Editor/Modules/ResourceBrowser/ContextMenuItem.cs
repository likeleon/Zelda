﻿using System;
using System.Windows.Input;

namespace Zelda.Editor.Modules.ResourceBrowser
{
    class ContextMenuItem
    {
        public string Text { get; set; }
        public Uri IconSource { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
    }
}
