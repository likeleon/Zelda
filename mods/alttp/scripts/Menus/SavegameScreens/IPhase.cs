using Zelda.Game;
using Zelda.Game.Engine;

namespace Alttp.Menus.SavegameScreens
{
    interface IPhase
    {
        string Name { get; }
        void OnDraw(SavegameScreen screen);
        bool DirectionPressed(SavegameScreen screen, Direction8 direction8);
        bool KeyPressed(SavegameScreen screen, KeyboardKey key);
    }
}
