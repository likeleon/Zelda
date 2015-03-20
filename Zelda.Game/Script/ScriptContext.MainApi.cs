using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        internal static void MainOnDraw(Surface dstSurface)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _scriptMain.OnDraw(dstSurface);
            });
            MenusOnDraw(_scriptMain, dstSurface);
        }

        internal static bool MainOnInput(InputEvent inputEvent)
        {
            bool handled = OnInput(_scriptMain, inputEvent);
            if (!handled)
                handled = MenusOnInput(_scriptMain, inputEvent);
            return handled;
        }
    }
}
