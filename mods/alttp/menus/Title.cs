using System;
using Zelda.Game;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Alttp.Menus
{
    class Title : ScriptMenu
    {
        enum Phase
        {
            Black,
            ZsPresents,
            Title
        }

        readonly bool _debugEnabled;

        Phase _phase;
        ScriptSurface _surface;
        ScriptSurface _zsPresentsImg;
        Point _zsPresentsPos;
        ScriptSurface _backgroundImg;
        ScriptSurface _cloudsImg;
        ScriptSurface _logoImg;
        ScriptSurface _bordersImg;
        ScriptTextSurface _websiteImg;
        ScriptTextSurface _pressSpaceImg;
        ScriptSurface _dxImg;
        ScriptSurface _starImg;
        bool _showPressSpace;
        Point _cloudsXY;
        bool _allowSkip;
        bool _finished;

        public Title(bool debugEnabled)
        {
            _debugEnabled = debugEnabled;
        }

        protected override void OnStarted()
        {
            // 0.3초 동안 검정 스크린을 유지합니다.
            _phase = Phase.Black;

            _surface = ScriptSurface.Create(320, 240);
            ScriptTimer.Start(this, 300, (Action)PhaseZsPresents);

            // 0.3초의 시간 동안 사운드 이펙트들을 미리 로딩합니다.
            ScriptAudio.PreloadSounds();
        }

        void PhaseZsPresents()
        {
            _phase = Phase.ZsPresents;

            _zsPresentsImg = ScriptSurface.Create("title_screen_initialization.png", true);
            
            _zsPresentsPos = new Point(160 - _zsPresentsImg.Width / 2, 120 - _zsPresentsImg.Height / 2);
            ScriptAudio.PlaySound("intro");

            ScriptTimer.Start(this, 2000, () =>
            {
                _surface.FadeOut(10);
                ScriptTimer.Start(this, 700, (Action)PhaseTitle);
            });
        }

        void PhaseTitle()
        {
            _phase = Phase.Title;

            ScriptAudio.PlayMusic("title_screen");

            var hours = DateTime.Now.Hour;
            var timeOfDay = string.Empty;
            if (hours >= 8 && hours < 18)
                timeOfDay = "daylight";
            else if (hours >= 18 && hours < 20)
                timeOfDay = "sunset";
            else
                timeOfDay = "night";

            _backgroundImg = ScriptSurface.Create("menus/title_{0}_background.png".F(timeOfDay));
            _cloudsImg = ScriptSurface.Create("menus/title_{0}_clouds.png".F(timeOfDay));
            _logoImg = ScriptSurface.Create("menus/title_logo.png");
            _bordersImg = ScriptSurface.Create("menus/title_borders.png");

            var dialogFont = LanguageFonts.GetDialogFont();
            var menuFont = LanguageFonts.GetMenuFont();
            
            _websiteImg = ScriptTextSurface.Create(
                font: menuFont.Item1,
                fontSize: menuFont.Item2,
                color: new Color(240, 200, 56),
                textKey: "title_screen.website",
                horizontalAlignment: TextHorizontalAlignment.Center);

            _pressSpaceImg = ScriptTextSurface.Create(
                font: dialogFont.Item1,
                fontSize: dialogFont.Item2,
                color: Color.White,
                textKey: "title_screen.press_space",
                horizontalAlignment: TextHorizontalAlignment.Center);

            ScriptTimer.Start(this, 5000, () =>
            {
                ScriptAudio.PlaySound("ok");
                _dxImg = ScriptSurface.Create("menus/title_dx.png");
            });

            ScriptTimer.Start(this, 6000, () =>
            {
                _starImg = ScriptSurface.Create("menus/title_star.png");
            });

            Action switchPressSpace = null;
            switchPressSpace = () =>
            {
                _showPressSpace = !_showPressSpace;
                ScriptTimer.Start(this, 500, switchPressSpace);
            };
            ScriptTimer.Start(this, 6500, switchPressSpace);

            _cloudsXY = new Point(320, 240);
            Action moveClouds = null;
            moveClouds = () =>
            {
                _cloudsXY.X += 1;
                _cloudsXY.Y -= 1;
                if (_cloudsXY.X >= 535)
                    _cloudsXY.X -= 535;
                if (_cloudsXY.Y < 0)
                    _cloudsXY.Y += 299;
                ScriptTimer.Start(this, 50, moveClouds);
            };
            ScriptTimer.Start(this, 50, moveClouds);

            _surface.FadeIn(30);

            ScriptTimer.Start(this, 2000, () => _allowSkip = true);
        }

        protected override void OnDraw(ScriptSurface dstSurface)
        {
            if (_phase == Phase.Title)
                DrawPhaseTitle();
            else if (_phase == Phase.ZsPresents)
                DrawPhasePresent();

            _surface.Draw(dstSurface, dstSurface.Width / 2 - 160, dstSurface.Height / 2 - 120);
        }

        void DrawPhasePresent()
        {
            _zsPresentsImg.Draw(_surface, _zsPresentsPos);
        }

        void DrawPhaseTitle()
        {
            _surface.FillColor(Color.Black);
            _backgroundImg.Draw(_surface);

            _cloudsImg.Draw(_surface, _cloudsXY.X, _cloudsXY.Y);
            _cloudsImg.Draw(_surface, _cloudsXY.X - 535, _cloudsXY.Y);
            _cloudsImg.Draw(_surface, _cloudsXY.X, _cloudsXY.Y - 299);
            _cloudsImg.Draw(_surface, _cloudsXY.X - 535, _cloudsXY.Y - 299);

            _bordersImg.Draw(_surface, 0, 0);

            _websiteImg.Draw(_surface, 160, 220);
            _logoImg.Draw(_surface);

            if (_dxImg != null)
                _dxImg.Draw(_surface);

            if (_starImg != null)
                _starImg.Draw(_surface);

            if (_showPressSpace)
                _pressSpaceImg.Draw(_surface, 160, 200);
        }

        public override bool OnKeyPressed(KeyboardKey key, Modifiers modifiers)
        {
            var handled = false;

            if (key == KeyboardKey.KEY_ESCAPE)
            {
                Main.Exit();
                handled = true;
            }
            else if (key == KeyboardKey.KEY_SPACE || key == KeyboardKey.KEY_RETURN)
            {
                handled = TryFinishTitle();
            }
            else if (_debugEnabled)
            {
                if (key == KeyboardKey.KEY_LEFT_SHIFT || key == KeyboardKey.KEY_RIGHT_SHIFT)
                {
                    FinishTitle();
                    handled = true;
                }
            }

            return handled;
        }

        bool TryFinishTitle()
        {
            if (_phase == Phase.Title &&
                _allowSkip &&
                !_finished)
            {
                _finished = true;

                _surface.FadeOut(30);
                ScriptTimer.Start(this, 700, (Action)FinishTitle);
                return true;
            }

            return false;
        }

        void FinishTitle()
        {
            ScriptAudio.StopMusic();
            ScriptMenu.Stop(this);
        }
    }
}
