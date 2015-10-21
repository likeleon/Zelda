﻿using Caliburn.Micro;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Zelda.Editor.Core.Commands;
using Zelda.Editor.Core.ToolBars;

namespace Zelda.Editor.Modules.ToolBars.Models
{
    class CommandToolBarItem : ToolBarItemBase, ICommandUiItem
    {
        readonly ToolBarItemDefinition _toolBarItem;
        readonly Command _command;
        readonly IToolBar _parent;

        public string Text { get { return _command.Text; } }
        public ToolBarItemDisplay Display { get { return _toolBarItem.Display; } }
        public Uri IconSource { get { return _command.IconSource; } }

        public string ToolTip
        {
            get
            {
                var inputGestureText = (_command.KeyGesture != null)
                    ? string.Format(" ({0})", _command.KeyGesture.GetDisplayStringForCulture(CultureInfo.CurrentUICulture))
                    : string.Empty;

                return string.Format("{0}{1}", _command.ToolTip, inputGestureText).Trim();
            }
        }

        public bool HasToolTip { get { return !string.IsNullOrWhiteSpace(ToolTip); } }
        public ICommand Command { get { return IoC.Get<ICommandService>().GetTargetableCommand(_command); } }
        public bool IsChecked { get { return _command.Checked; } }

        public CommandToolBarItem(ToolBarItemDefinition toolBarItem, Command command, IToolBar parent)
        {
            _toolBarItem = toolBarItem;
            _command = command;
            _parent = parent;

            command.PropertyChanged += OnCommandPropertyChanged;
        }

        void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => Text);
            NotifyOfPropertyChange(() => IconSource);
            NotifyOfPropertyChange(() => ToolTip);
            NotifyOfPropertyChange(() => HasToolTip);
            NotifyOfPropertyChange(() => IsChecked);
        }

        CommandDefinitionBase ICommandUiItem.CommandDefinition { get { return _command.CommandDefinition; } }

        void ICommandUiItem.Update(CommandHandlerWrapper commandHandler)
        {
            // TODO
        }
    }
}