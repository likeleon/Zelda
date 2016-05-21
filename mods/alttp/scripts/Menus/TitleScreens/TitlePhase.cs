using System;
using Zelda.Game;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.TitleScreens
{
    class TitlePhase : IPhase
    {
        readonly TitleScreen _titleScreen;
        readonly Surface _backgroundImg;
        readonly Surface _cloudsImg;
        readonly Surface _logoImg;
        readonly Surface _bordersImg;
        readonly TextSurface _websiteImg;
        readonly TextSurface _pressSpaceImg;
        Surface _dxImg;
        Surface _starImg;
        bool _showPressSpace;
        Point _cloudsXY;
        bool _allowSkip;
        bool _finished;

        public event EventHandler Finished = delegate { };

        public TitlePhase(TitleScreen titleScreen)
        {
            _titleScreen = titleScreen;

            Core.Audio?.PlayMusic("title_screen");

            var hours = DateTime.Now.Hour;
            var timeOfDay = string.Empty;
            if (hours >= 8 && hours < 18)
                timeOfDay = "daylight";
            else if (hours >= 18 && hours < 20)
                timeOfDay = "sunset";
            else
                timeOfDay = "night";

            _backgroundImg = Surface.Create("menus/title_{0}_background.png".F(timeOfDay), true);
            _cloudsImg = Surface.Create("menus/title_{0}_clouds.png".F(timeOfDay), true);
            _logoImg = Surface.Create("menus/title_logo.png", true);
            _bordersImg = Surface.Create("menus/title_borders.png", true);

            var dialogFont = Fonts.GetDialogFont();
            var menuFont = Fonts.GetMenuFont();

            _websiteImg = TextSurface.Create(
                font: menuFont.Id,
                fontSize: menuFont.Size,
                color: new Color(240, 200, 56),
                textKey: "title_screen.website",
                horizontalAlignment: TextHorizontalAlignment.Center);

            _pressSpaceImg = TextSurface.Create(
                font: dialogFont.Id,
                fontSize: dialogFont.Size,
                color: Color.White,
                textKey: "title_screen.press_space",
                horizontalAlignment: TextHorizontalAlignment.Center);

            Timer.Start(_titleScreen, 5000, () =>
            {
                Core.Audio?.PlaySound("ok");
                _dxImg = Surface.Create("menus/title_dx.png", true);
            });

            Timer.Start(_titleScreen, 6000, () =>
            {
                _starImg = Surface.Create("menus/title_star.png", true);
            });

            Action switchPressSpace = null;
            switchPressSpace = () =>
            {
                _showPressSpace = !_showPressSpace;
                Timer.Start(_titleScreen, 500, switchPressSpace);
            };
            Timer.Start(_titleScreen, 6500, switchPressSpace);

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
                Timer.Start(_titleScreen, 50, moveClouds);
            };
            Timer.Start(_titleScreen, 50, moveClouds);

            _titleScreen.Surface.FadeIn(30);

            Timer.Start(_titleScreen, 2000, () => _allowSkip = true);
        }

        public void OnDraw(Surface dstSurface)
        {
            _titleScreen.Surface.FillWithColor(Color.Black);
            _backgroundImg.Draw(_titleScreen.Surface);

            _cloudsImg.Draw(_titleScreen.Surface, _cloudsXY.X, _cloudsXY.Y);
            _cloudsImg.Draw(_titleScreen.Surface, _cloudsXY.X - 535, _cloudsXY.Y);
            _cloudsImg.Draw(_titleScreen.Surface, _cloudsXY.X, _cloudsXY.Y - 299);
            _cloudsImg.Draw(_titleScreen.Surface, _cloudsXY.X - 535, _cloudsXY.Y - 299);

            _bordersImg.Draw(_titleScreen.Surface, 0, 0);

            _websiteImg.Draw(_titleScreen.Surface, 160, 220);
            _logoImg.Draw(_titleScreen.Surface);

            if (_dxImg != null)
                _dxImg.Draw(_titleScreen.Surface);

            if (_starImg != null)
                _starImg.Draw(_titleScreen.Surface);

            if (_showPressSpace)
                _pressSpaceImg.Draw(_titleScreen.Surface, 160, 200);
        }

        public bool TryFinishTitle()
        {
            if (_allowSkip && !_finished)
            {
                _finished = true;

                _titleScreen.Surface.FadeOut(30);
                Timer.Start(_titleScreen, 700, () => Finished(this, EventArgs.Empty));
                return true;
            }
            return false;
        }
    }
}
