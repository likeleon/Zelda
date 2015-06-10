using System;
using System.Linq;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class OptionsPhase : IPhase
    {
        abstract class Option
        {
            protected readonly OptionsPhase _phase;

            public abstract string Name { get; }
            public ScriptTextSurface LabelText { get; private set; }
            public ScriptTextSurface ValueText { get; private set; }
            public string[] Values { get; protected set; }
            public string InitialValue { get; protected set; }
            public int CurrentIndex { get; set; }

            public Option(OptionsPhase phase)
            {
                _phase = phase;

                var font = LanguageFonts.GetMenuFont();
                LabelText = ScriptTextSurface.Create(
                    font: font.Item1,
                    fontSize: font.Item2,
                    textKey: "selection_menu.options.{0}".F(Name));
                ValueText = ScriptTextSurface.Create(
                    font: font.Item1,
                    fontSize: font.Item2,
                    horizontalAlignment: TextHorizontalAlignment.Right);

                CurrentIndex = -1;                
            }

            public void SetValue(int index)
            {
                if (CurrentIndex == index)
                    return;

                CurrentIndex = index;
                
                ApplyValue(Values[index]);
            }

            protected abstract void ApplyValue(string value);
        }

        class LanguageOption : Option
        {
            public override string Name { get { return "language"; } }

            public LanguageOption(OptionsPhase phase)
                : base(phase)
            {
                Values = ScriptLanguage.Languages.ToArray();
                InitialValue = ScriptLanguage.Language;
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(ScriptLanguage.GetLanguageName(value));
                if (value != ScriptLanguage.Language)
                {
                    ScriptLanguage.SetLanguage(value);
                    _phase.ReloadOptionsStrings();
                }
            }
        }

        class VideoModeOption : Option
        {
            public override string Name { get { return "video_mode"; } }

            public VideoModeOption(OptionsPhase phase)
                : base(phase)
            {
                Values = new string[] { };
                InitialValue = string.Empty;
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(value);
            }
        }

        class MusicVolumeOption : Option
        {
            public override string Name { get { return "music_volume"; } }

            public MusicVolumeOption(OptionsPhase phase)
                : base(phase)
            {
                Values = (new[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }).Select(i => i.ToString()).ToArray();
                InitialValue = Convert.ToString(((ScriptAudio.MusicVolume + 5) / 10) * 10);
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(value);
                ScriptAudio.SetMusicVolume(int.Parse(value));
            }
        }

        class SoundVolumeOption : Option
        {
            public override string Name { get { return "sound_volume"; } }

            public SoundVolumeOption(OptionsPhase phase)
                : base(phase)
            {
                Values = (new[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }).Select(i => i.ToString()).ToArray();
                InitialValue = Convert.ToString(((ScriptAudio.SoundVolume + 5) / 10) * 10);
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(value);
                ScriptAudio.SetSoundVolume(int.Parse(value));
            }
        }

        readonly SavegameScreen _screen;
        readonly Option[] _options;
        readonly ScriptSprite _leftArrowSprite;
        readonly ScriptSprite _rightArrowSprite;

        bool _modifyingOption;
        int _optionsCursorPosition = 1;

        public string Name { get { return "options"; } }

        public OptionsPhase(SavegameScreen screen)
        {
            _screen = screen;

            _screen.TitleText.SetTextKey("selection_menu.phase.options");

            _options = new Option[] { new LanguageOption(this), new VideoModeOption(this), new MusicVolumeOption(this), new SoundVolumeOption(this) };
            foreach (var option in _options)
                for (int i = 0; i < option.Values.Length; ++i)
                    if (option.Values[i] == option.InitialValue)
                        option.SetValue(i);

            _leftArrowSprite = ScriptSprite.Create("menus/arrow");
            _leftArrowSprite.SetAnimation("blink");
            _leftArrowSprite.SetDirection(Direction4.Left);

            _rightArrowSprite = ScriptSprite.Create("menus/arrow");
            _rightArrowSprite.SetAnimation("blink");
            _rightArrowSprite.SetDirection(Direction4.Right);

            _screen.SetBottomButtons("selection_menu.back", null);
            SetOptionsCursorPosition(1);
        }

        public void OnDraw()
        {
            for (int i = 0; i < _options.Length; ++i)
            {
                var y = 70 + (i + 1) * 16;
                _options[i].LabelText.Draw(_screen.Surface, 64, y);
                _options[i].ValueText.Draw(_screen.Surface, 266, y);
            }

            _screen.DrawBottomButtons();

            if (_optionsCursorPosition > _options.Length)
                _screen.DrawSavegameCursor();
            else
            {
                var y = 64 + _optionsCursorPosition * 16;
                if (_modifyingOption)
                {
                    var option = _options[_optionsCursorPosition - 1];
                    var width = option.ValueText.Size.Width;
                    _leftArrowSprite.Draw(_screen.Surface, 256 - width, y);
                    _rightArrowSprite.Draw(_screen.Surface, 268, y);
                }
                else
                    _rightArrowSprite.Draw(_screen.Surface, 54, y);
            }
        }

        void SetOptionsCursorPosition(int position)
        {
            if (_optionsCursorPosition <= _options.Length)
            {
                var option = _options[_optionsCursorPosition - 1];
                option.LabelText.SetColor(Color.White);
            }

            _optionsCursorPosition = position;
            if (position > _options.Length)
                _screen.SetCursorPosition(4);

            if (position <= _options.Length)
            {
                var option = _options[_optionsCursorPosition - 1];
                option.LabelText.SetColor(Color.Yellow);
            }
        }

        public bool DirectionPressed(Zelda.Game.Direction8 direction8)
        {
            return false;
        }

        public bool KeyPressed(Zelda.Game.Engine.KeyboardKey key)
        {
            return false;
        }

        void ReloadOptionsStrings()
        {
            var menuFont = LanguageFonts.GetMenuFont();
            var dialogFont = LanguageFonts.GetDialogFont();

            foreach (var option in _options)
            {
                option.LabelText.SetFont(menuFont.Item1);
                option.LabelText.SetFontSize(menuFont.Item2);
                option.ValueText.SetFont(menuFont.Item1);
                option.ValueText.SetFontSize(menuFont.Item2);
                option.LabelText.SetTextKey("selection_menu.options.{0}".F(Name));

                if (option is VideoModeOption && option.CurrentIndex != -1)
                {
                    var mode = option.Values[option.CurrentIndex];
                    option.ValueText.SetText(mode);
                }
            }

            _screen.TitleText.SetTextKey("selection_menu.phase.options");
            _screen.TitleText.SetFont(menuFont.Item1);
            _screen.TitleText.SetFontSize(menuFont.Item2);
            _screen.Option1Text.SetFont(dialogFont.Item1);
            _screen.Option1Text.SetFontSize(dialogFont.Item2);
            _screen.Option2Text.SetFont(dialogFont.Item1);
            _screen.Option2Text.SetFontSize(dialogFont.Item2);
            _screen.SetBottomButtons("selection_menu.back", null);
            _screen.ReadSavegames();    // - Empty - 텍스트를 갱신하기 위해서 필요합니다
        }
    }
}
