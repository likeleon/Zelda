using Zelda.Game;
using Zelda.Game.Lowlevel;

namespace Alttp.Menus.SavegameScreens
{
    interface IPhase
    {
        string Name { get; }
        
        void OnDraw();
        bool DirectionPressed(Direction8 direction8);
        bool KeyPressed(KeyboardKey key);
    }
}
