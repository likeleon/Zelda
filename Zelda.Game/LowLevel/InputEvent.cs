using SDL2;

namespace Zelda.Game.LowLevel
{
    class InputEvent
    {
        public bool IsWindowClosing { get { return _internalEvent.type == SDL.SDL_EventType.SDL_QUIT; } }

        public string Character
        {
            get
            {
                unsafe
                {
                    var text = _internalEvent.text;
                    return new string((sbyte*)text.text);
                }
            }
        }

        public bool KeyRepeat { set; private get; }

        public bool IsKeyboardEvent
        {
            get 
            {
                return (_internalEvent.type == SDL.SDL_EventType.SDL_KEYDOWN ||
                        _internalEvent.type == SDL.SDL_EventType.SDL_KEYUP) &&
                       (_internalEvent.key.repeat == 0 || KeyRepeat);
            }
        }

        public bool IsKeyboardKeyPressed
        {
            get
            {
                return _internalEvent.type == SDL.SDL_EventType.SDL_KEYDOWN &&
                       (_internalEvent.key.repeat == 0 || KeyRepeat);
            }
        }

        public bool IsKeyboardKeyReleased
        {
            get
            {
                return _internalEvent.type == SDL.SDL_EventType.SDL_KEYUP &&
                       (_internalEvent.key.repeat == 0 || KeyRepeat);
            }
        }

        public bool IsCharacterPressed
        {
            get { return _internalEvent.type == SDL.SDL_EventType.SDL_TEXTINPUT; }
        }

        public bool IsWithShift
        {
            get 
            { 
                return IsKeyboardEvent && 
                       (_internalEvent.key.keysym.mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0; 
            }
        }

        public bool IsWithControl
        {
            get
            {
                return IsKeyboardEvent &&
                       (_internalEvent.key.keysym.mod & SDL.SDL_Keymod.KMOD_CTRL) != 0;
            }
        }

        public bool IsWithAlt
        {
            get
            {
                return IsKeyboardEvent &&
                       (_internalEvent.key.keysym.mod & SDL.SDL_Keymod.KMOD_ALT) != 0;
            }
        }

        public KeyboardKey KeyboardKey
        {
            get
            {
                if (!IsKeyboardEvent)
                    return KeyboardKey.None;

                return (KeyboardKey)_internalEvent.key.keysym.sym;
            }
        }


        readonly SDL.SDL_Event _internalEvent;

        public InputEvent(SDL.SDL_Event evt)
        {
            _internalEvent = evt;
        }
    }
}
