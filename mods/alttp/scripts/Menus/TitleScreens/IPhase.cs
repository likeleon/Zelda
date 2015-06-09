using System;
using Zelda.Game.Script;

namespace Alttp.Menus.TitleScreens
{
    interface IPhase
    {
        event EventHandler Finished;
        
        void OnDraw(ScriptSurface dstSurface);
        bool TryFinishTitle();
    }
}
