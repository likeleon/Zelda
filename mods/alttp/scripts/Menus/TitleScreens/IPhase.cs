using System;
using Zelda.Game.LowLevel;

namespace Alttp.Menus.TitleScreens
{
    interface IPhase
    {
        event EventHandler Finished;
        
        void OnDraw(Surface dstSurface);
        bool TryFinishTitle();
    }
}
