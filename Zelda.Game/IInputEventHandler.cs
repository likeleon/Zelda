using Zelda.Game.LowLevel;

namespace Zelda.Game
{
    public class Modifiers
    {
        public bool Shift { get; }
        public bool Control { get; }
        public bool Alt { get; }

        internal Modifiers(InputEvent inputEvent)
        {
            Shift = inputEvent.IsWithShift;
            Control = inputEvent.IsWithControl;
            Alt = inputEvent.IsWithAlt;
        }
    }

    public interface IInputEventHandler
    {
        bool OnKeyPressed(KeyboardKey key, Modifiers modifiers);
        bool OnKeyReleased(KeyboardKey key);
        bool OnCharacterPressed(string character);
    }

    internal static class IInputEventHandlerExts
    {
        internal static bool OnInput(this IInputEventHandler handler, InputEvent input)
        {
            bool handled = false;
            if (input.IsKeyboardEvent)
            {
                if (input.IsKeyboardKeyPressed)
                    handled = OnKeyPressed(handler, input) || handled;
                else if (input.IsKeyboardKeyReleased)
                    handled = OnKeyReleased(handler, input) || handled;
            }
            else if (input.IsCharacterPressed)
                handled = OnCharacterPressed(handler, input) || handled;

            return handled;
        }

        static bool OnKeyPressed(IInputEventHandler handler, InputEvent input) => handler.OnKeyPressed(input.KeyboardKey, new Modifiers(input));
        static bool OnKeyReleased(IInputEventHandler handler, InputEvent input) => handler.OnKeyReleased(input.KeyboardKey);
        static bool OnCharacterPressed(IInputEventHandler handler, InputEvent input) => handler.OnCharacterPressed(input.Character);
    }
}
