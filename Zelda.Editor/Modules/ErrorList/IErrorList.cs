﻿using Caliburn.Micro;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.ErrorList
{
    public interface IErrorList : ITool
    {
        bool ShowErrors { get; set; }
        bool ShowWarnings { get; set; }
        bool ShowMessages { get; set; }

        IObservableCollection<ErrorListItem> Items { get; }

        void AddItem(ErrorListItemType itemType, string description,
            string path = null, int? line = null, int? column = null,
            System.Action onClick = null);
    }
}
