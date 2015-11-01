using Zelda.Editor.Core;
using Zelda.Editor.Core.Services;
using Zelda.Editor.Modules.StringsEditor.Models;
using Zelda.Game;

namespace Zelda.Editor.Modules.StringsEditor.ViewModels
{
    class NewStringDialogViewModel : WindowBase
    {
        readonly StringsModel _model;
        string _stringKey;
        string _stringValue;

        public string StringKey
        {
            get { return _stringKey; }
            set { this.SetProperty(ref _stringKey, value); }
        }

        public string StringValue
        {
            get { return _stringValue; }
            set { this.SetProperty(ref _stringValue, value); }
        }

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public NewStringDialogViewModel(StringsModel model, string initialKey, string initialValue = "")
        {
            _model = model;
            _stringKey = initialKey;
            _stringValue = initialValue;

            OkCommand = new RelayCommand(_ => OnOkExecute());
            CancelCommand = new RelayCommand(_ => TryClose(false));
        }

        void OnOkExecute()
        {
            if (!StringsModel.IsValidKey(StringKey))
            {
                "Invalid string key".ShowErrorDialog();
                return;
            }

            if (_model.StringExists(StringKey))
            {
                "This string key already exists".ShowErrorDialog();
                return;
            }

            if (StringValue.IsNullOrEmpty())
            {
                "Value cannot be empty".ShowErrorDialog();
                return;
            }

            TryClose(true);
        }
    }
}
