using System.Linq;
using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.SavegameScreens
{
    class OptionsPhase : IPhase
    {
        abstract class Option
        {
            protected readonly OptionsPhase _phase;

            public abstract string Name { get; }
            public TextSurface LabelText { get; private set; }
            public TextSurface ValueText { get; private set; }
            public string[] Values { get; protected set; }
            public string InitialValue { get; protected set; }
            public int CurrentIndex { get; set; }

            public Option(OptionsPhase phase)
            {
                _phase = phase;

                var font = Fonts.GetMenuFont();
                LabelText = TextSurface.Create(
                    font: font.Id,
                    fontSize: font.Size,
                    textKey: "selection_menu.options.{0}".F(Name));
                ValueText = TextSurface.Create(
                    font: font.Id,
                    fontSize: font.Size,
                    horizontalAlignment: TextHorizontalAlignment.Right);

                CurrentIndex = 0;                
            }

            public void SetValue(int index)
            {
                if (CurrentIndex == index)
                    return;

                CurrentIndex = index;
                
                ApplyValue(Values[index - 1]);
            }

            protected abstract void ApplyValue(string value);
        }

        class LanguageOption : Option
        {
            public override string Name => "language";

            public LanguageOption(OptionsPhase phase)
                : base(phase)
            {
                Values = Core.Mod.Languages.ToArray();
                InitialValue = Core.Mod.Language;
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(Core.Mod.GetLanguageName(value));
                if (value != Core.Mod.Language)
                {
                    Core.Mod.SetLanguage(value);
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
                Values = new string[] { "Normal" };
                InitialValue = Values[0];
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
                if (Core.Audio != null)
                    InitialValue = (((Core.Audio?.MusicVolume + 5) / 10) * 10).ToString();
                else
                    InitialValue = Values[0];
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(value);
                if (Core.Audio != null)
                    Core.Audio.MusicVolume = int.Parse(value);
            }
        }

        class SoundVolumeOption : Option
        {
            public override string Name { get { return "sound_volume"; } }

            public SoundVolumeOption(OptionsPhase phase)
                : base(phase)
            {
                Values = (new[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }).Select(i => i.ToString()).ToArray();
                if (Core.Audio != null)
                    InitialValue = (((Core.Audio?.SoundVolume + 5) / 10) * 10).ToString();
                else
                    InitialValue = Values[0];
            }

            protected override void ApplyValue(string value)
            {
                ValueText.SetText(value);
                if (Core.Audio != null)
                    Core.Audio.SoundVolume = int.Parse(value);
            }
        }

        readonly SavegameScreen _screen;
        readonly Option[] _options;
        readonly Sprite _leftArrowSprite;
        readonly Sprite _rightArrowSprite;

        bool _modifyingOption;
        int _optionsCursorPosition = 1;

        public string Name { get { return "options"; } }

        public OptionsPhase(SavegameScreen screen)
        {
            _screen = screen;

            _screen.TitleText.SetTextKey("selection_menu.phase.options");

            _options = new Option[] { new LanguageOption(this), new VideoModeOption(this), new MusicVolumeOption(this), new SoundVolumeOption(this) };
            foreach (var option in _options)
                for (int i = 1; i <= option.Values.Length; ++i)
                    if (option.Values[i - 1] == option.InitialValue)
                        option.SetValue(i);

            _leftArrowSprite = Sprite.Create("menus/arrow", true);
            _leftArrowSprite.SetAnimation("blink");
            _leftArrowSprite.SetCurrentDirection(Direction4.Left);

            _rightArrowSprite = Sprite.Create("menus/arrow", true);
            _rightArrowSprite.SetAnimation("blink");
            _rightArrowSprite.SetCurrentDirection(Direction4.Right);

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
                option.LabelText.SetTextColor(Color.White);
            }

            _optionsCursorPosition = position;
            if (position > _options.Length)
                _screen.SetCursorPosition(4);

            if (position <= _options.Length)
            {
                var option = _options[_optionsCursorPosition - 1];
                option.LabelText.SetTextColor(Color.Yellow);
            }
        }

        public bool DirectionPressed(Zelda.Game.Direction8 direction8)
        {
            if (!_modifyingOption)
            {
                if (direction8 == Direction8.Up)
                {
                    Core.Audio?.PlaySound("cursor");
                    _leftArrowSprite.SetCurrentFrame(0);
                    var position = _optionsCursorPosition - 1;
                    if (position == 0)
                        position = _options.Length + 1;
                    SetOptionsCursorPosition(position);
                    return true;
                }
                else if (direction8 == Direction8.Down)
                {
                    Core.Audio?.PlaySound("cursor");
                    _leftArrowSprite.SetCurrentFrame(0);
                    var position = _optionsCursorPosition + 1;
                    if (position > _options.Length + 1)
                        position = 1;
                    SetOptionsCursorPosition(position);
                    return true;
                }
            }
            else
            {
                if (direction8 == Direction8.Right)
                {
                    var option = _options[_optionsCursorPosition - 1];
                    var index = (option.CurrentIndex % option.Values.Length) + 1;
                    option.SetValue(index);
                    Core.Audio?.PlaySound("cursor");
                    _leftArrowSprite.SetCurrentFrame(0);
                    _rightArrowSprite.SetCurrentFrame(0);
                    return true;
                }
                else if (direction8 == Direction8.Left)
                {
                    var option = _options[_optionsCursorPosition - 1];
                    var index = (option.CurrentIndex + option.Values.Length - 2) % option.Values.Length + 1;
                    option.SetValue(index);
                    Core.Audio?.PlaySound("cursor");
                    _leftArrowSprite.SetCurrentFrame(0);
                    _rightArrowSprite.SetCurrentFrame(0);
                    return true;
                }
            }
            return false;
        }

        public bool KeyPressed(KeyboardKey key)
        {
            if (key != KeyboardKey.Space && key != KeyboardKey.Return)
                return false;

            if (_optionsCursorPosition > _options.Length)
            {
                Core.Audio?.PlaySound("ok");
                _screen.InitPhaseSelectFile();
            }
            else
            {
                var option = _options[_optionsCursorPosition - 1];
                if (!_modifyingOption)
                {
                    Core.Audio?.PlaySound("ok");
                    _leftArrowSprite.SetCurrentFrame(0);
                    _rightArrowSprite.SetCurrentFrame(0);
                    option.LabelText.SetTextColor(Color.White);
                    option.ValueText.SetTextColor(Color.Yellow);
                    _screen.TitleText.SetTextKey("selection_menu.phase.options.changing");
                    _modifyingOption = true;
                }
                else
                {
                    Core.Audio?.PlaySound("danger");
                    option.LabelText.SetTextColor(Color.Yellow);
                    option.ValueText.SetTextColor(Color.White);
                    _leftArrowSprite.SetCurrentFrame(0);
                    _rightArrowSprite.SetCurrentFrame(0);
                    _screen.TitleText.SetTextKey("selection_menu.phase.options");
                    _modifyingOption = false;
                }
            }
            return true;
        }

        void ReloadOptionsStrings()
        {
            var menuFont = Fonts.GetMenuFont();
            var dialogFont = Fonts.GetDialogFont();

            foreach (var option in _options)
            {
                option.LabelText.SetFont(menuFont.Id);
                option.LabelText.SetFontSize(menuFont.Size);
                option.ValueText.SetFont(menuFont.Id);
                option.ValueText.SetFontSize(menuFont.Size);
                option.LabelText.SetTextKey("selection_menu.options.{0}".F(Name));

                if (option is VideoModeOption && option.CurrentIndex != 0)
                {
                    var mode = option.Values[option.CurrentIndex - 1];
                    option.ValueText.SetText(mode);
                }
            }

            _screen.TitleText.SetTextKey("selection_menu.phase.options");
            _screen.TitleText.SetFont(menuFont.Id);
            _screen.TitleText.SetFontSize(menuFont.Size);
            _screen.Option1Text.SetFont(dialogFont.Id);
            _screen.Option1Text.SetFontSize(dialogFont.Size);
            _screen.Option2Text.SetFont(dialogFont.Id);
            _screen.Option2Text.SetFontSize(dialogFont.Size);
            _screen.SetBottomButtons("selection_menu.back", null);
            _screen.ReadSavegames();    // - Empty - 텍스트를 갱신하기 위해서 필요합니다
        }
    }
}
