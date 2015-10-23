using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.DialogsEditor.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.ViewModels
{
    class ChangeDialogIdViewModel : WindowBase
    {
        readonly DialogsModel _model;
        readonly string _initialId;
        bool _isPrefix;
        string _dialogId;
        
        public bool AllowPrefix { get; private set; }

        public bool IsPrefix
        {
            get { return _isPrefix; }
            set { this.SetProperty(ref _isPrefix, value); }
        }

        public string DialogId
        {
            get { return _dialogId; }
            set { this.SetProperty(ref _dialogId, value); }
        }

        public string Label { get; private set; }

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public ChangeDialogIdViewModel(DialogsModel model, string initialId, bool isPrefix, bool allowPrefix)
        {
            _model = model;
            _initialId = initialId;
            DialogId = initialId;
            AllowPrefix = allowPrefix;
            IsPrefix = isPrefix;

            if (!AllowPrefix && IsPrefix)
                Label = "New id for dialogs prefixed by '{0}':".F(_initialId);
            else
                Label = "New id for dialog '{0}'".F(_initialId);

            OkCommand = new RelayCommand(_ => OnOkExecute());
            CancelCommand = new RelayCommand(_ => TryClose(false));
        }

        void OnOkExecute()
        {
            if (!DialogsModel.IsValidId(DialogId))
            {
                "Invalid dialog id: {0}".F(DialogId).ShowErrorDialog();
                return;
            }

            if (DialogId != _initialId)
            {
                if (IsPrefix)
                {
                    string errorId;
                    if (!_model.CanSetDialogIdPrefix(_initialId, DialogId, out errorId))
                    {
                        "The dialog id '{0}' already exists".F(errorId).ShowErrorDialog();
                        return;
                    }
                }
                else if (_model.DialogExists(DialogId))
                {
                    "The dialog '{0}' already exists".F(DialogId).ShowErrorDialog();
                    return;
                }
            }

            TryClose(true);
        }
    }
}
