using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class Modifiers
    {
        public bool Shift { get; private set; }
        public bool Control { get; private set; }
        public bool Alt { get; private set; }

        internal Modifiers(InputEvent inputEvent)
        {
            Shift = inputEvent.IsWithShift;
            Control = inputEvent.IsWithControl;
            Alt = inputEvent.IsWithAlt;
        }
    }

    interface IInputEventHandler
    {
        bool OnKeyPressed(KeyboardKey key, Modifiers modifiers);
        bool OnKeyReleased(KeyboardKey key);
        bool OnCharacterPressed(string character);
    }
}
