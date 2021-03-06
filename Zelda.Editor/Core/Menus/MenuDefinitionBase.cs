﻿using System;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;

namespace Zelda.Editor.Core.Menus
{
    public abstract class MenuDefinitionBase
    {
        public abstract int SortOrder { get; }
        public abstract string Text { get; }
        public abstract Uri IconSource { get; }
        public abstract KeyGesture KeyGesture { get; }
        public abstract CommandDefinitionBase CommandDefinition { get; }
    }
}
