
namespace Zelda.Game.Script
{
    interface IInputEventHandler
    {
        bool OnKeyPressed(string key, bool shift, bool control, bool alt);
        bool OnKeyReleased(string key);
    }
}
