using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.StringsEditor.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.ViewModels
{
    class ChangeStringKeyViewModel : WindowBase
    {
        readonly StringsModel _model;
        readonly string _initialKey;
        bool _isPrefix;
        string _stringKey;

        public bool AllowPrefix { get; private set; }

        public bool IsPrefix
        {
            get { return _isPrefix; }
            set { this.SetProperty(ref _isPrefix, value); }
        }

        public string StringKey
        {
            get { return _stringKey; }
            set { this.SetProperty(ref _stringKey, value); }
        }

        public string Label { get; private set; }

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public ChangeStringKeyViewModel(StringsModel model, string initialKey, bool isPrefix, bool allowPrefix)
        {
            _model = model;
            _initialKey = initialKey;
            StringKey = initialKey;
            AllowPrefix = allowPrefix;
            IsPrefix = isPrefix;

            if (!AllowPrefix && IsPrefix)
                Label = "New key for strings prefixed by '{0}':".F(_initialKey);
            else
                Label = "New key for string '{0}'".F(_initialKey);

            OkCommand = new RelayCommand(_ => OnOkExecute());
            CancelCommand = new RelayCommand(_ => TryClose(false));
        }

        void OnOkExecute()
        {
            if (!StringsModel.IsValidKey(StringKey))
            {
                "Invalid string key: {0}".F(StringKey).ShowErrorDialog();
                return;
            }

            if (StringKey != _initialKey)
            {
                if (IsPrefix)
                {
                    string errorId;
                    if (!_model.CanSetStringKeyPrefix(_initialKey, StringKey, out errorId))
                    {
                        "The string key '{0}' already exists".F(errorId).ShowErrorDialog();
                        return;
                    }
                }
                else if (_model.StringExists(StringKey))
                {
                    "The string '{0}' already exists".F(StringKey).ShowErrorDialog();
                    return;
                }
            }

            TryClose(true);
        }
    }
}
