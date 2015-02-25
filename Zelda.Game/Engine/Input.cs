using SDL2;

namespace Zelda.Game.Engine
{
    public static class Input
    {
        public class Event
        {
            public bool IsWindowClosing
            {
                get { return _internalEvent.type == SDL.SDL_EventType.SDL_QUIT; }
            }

            private readonly SDL.SDL_Event _internalEvent;

            internal Event(SDL.SDL_Event sdlEvent)
            {
                _internalEvent = sdlEvent;
            }
        }

        enum KeyboardKey
        {
            KEY_NONE = SDL.SDL_Keycode.SDLK_UNKNOWN,
            KEY_BACKSPACE = SDL.SDL_Keycode.SDLK_BACKSPACE,
            KEY_TABULATION = SDL.SDL_Keycode.SDLK_TAB,
            KEY_CLEAR = SDL.SDL_Keycode.SDLK_CLEAR,
            KEY_RETURN = SDL.SDL_Keycode.SDLK_RETURN,
            KEY_PAUSE = SDL.SDL_Keycode.SDLK_PAUSE,
            KEY_ESCAPE = SDL.SDL_Keycode.SDLK_ESCAPE,
            KEY_SPACE = SDL.SDL_Keycode.SDLK_SPACE,
            KEY_EXCLAMATION_MARK = SDL.SDL_Keycode.SDLK_EXCLAIM,
            KEY_DOULE_QUOTE = SDL.SDL_Keycode.SDLK_QUOTEDBL,
            KEY_HASH = SDL.SDL_Keycode.SDLK_HASH,
            KEY_DOLLAR = SDL.SDL_Keycode.SDLK_DOLLAR,
            KEY_AMPERSAND = SDL.SDL_Keycode.SDLK_AMPERSAND,
            KEY_SINGLE_QUOTE = SDL.SDL_Keycode.SDLK_QUOTE,
            KEY_LEFT_PARENTHESIS = SDL.SDL_Keycode.SDLK_LEFTPAREN,
            KEY_RIGHT_PARENTHESIS = SDL.SDL_Keycode.SDLK_RIGHTPAREN,
            KEY_ASTERISK = SDL.SDL_Keycode.SDLK_ASTERISK,
            KEY_PLUS = SDL.SDL_Keycode.SDLK_PLUS,
            KEY_COMMA = SDL.SDL_Keycode.SDLK_COMMA,
            KEY_MINUS = SDL.SDL_Keycode.SDLK_MINUS,
            KEY_PERIOD = SDL.SDL_Keycode.SDLK_PERIOD,
            KEY_SLASH = SDL.SDL_Keycode.SDLK_SLASH,
            KEY_0 = SDL.SDL_Keycode.SDLK_0,
            KEY_1 = SDL.SDL_Keycode.SDLK_1,
            KEY_2 = SDL.SDL_Keycode.SDLK_2,
            KEY_3 = SDL.SDL_Keycode.SDLK_3,
            KEY_4 = SDL.SDL_Keycode.SDLK_4,
            KEY_5 = SDL.SDL_Keycode.SDLK_5,
            KEY_6 = SDL.SDL_Keycode.SDLK_6,
            KEY_7 = SDL.SDL_Keycode.SDLK_7,
            KEY_8 = SDL.SDL_Keycode.SDLK_8,
            KEY_9 = SDL.SDL_Keycode.SDLK_9,
            KEY_COLON = SDL.SDL_Keycode.SDLK_COLON,
            KEY_SEMICOLON = SDL.SDL_Keycode.SDLK_SEMICOLON,
            KEY_LESS = SDL.SDL_Keycode.SDLK_LESS,
            KEY_EQUALS = SDL.SDL_Keycode.SDLK_EQUALS,
            KEY_GREATER = SDL.SDL_Keycode.SDLK_GREATER,
            KEY_QUESTION_MARK = SDL.SDL_Keycode.SDLK_QUESTION,
            KEY_AT = SDL.SDL_Keycode.SDLK_AT,
            KEY_LEFT_BRACKET = SDL.SDL_Keycode.SDLK_LEFTBRACKET,
            KEY_BACKSLASH = SDL.SDL_Keycode.SDLK_BACKSLASH,
            KEY_RIGHT_BRACKET = SDL.SDL_Keycode.SDLK_RIGHTBRACKET,
            KEY_CARET = SDL.SDL_Keycode.SDLK_CARET,
            KEY_UNDERSCORE = SDL.SDL_Keycode.SDLK_UNDERSCORE,
            KEY_BACKQUOTE = SDL.SDL_Keycode.SDLK_BACKQUOTE,
            KEY_a = SDL.SDL_Keycode.SDLK_a,
            KEY_b = SDL.SDL_Keycode.SDLK_b,
            KEY_c = SDL.SDL_Keycode.SDLK_c,
            KEY_d = SDL.SDL_Keycode.SDLK_d,
            KEY_e = SDL.SDL_Keycode.SDLK_e,
            KEY_f = SDL.SDL_Keycode.SDLK_f,
            KEY_g = SDL.SDL_Keycode.SDLK_g,
            KEY_h = SDL.SDL_Keycode.SDLK_h,
            KEY_i = SDL.SDL_Keycode.SDLK_i,
            KEY_j = SDL.SDL_Keycode.SDLK_j,
            KEY_k = SDL.SDL_Keycode.SDLK_k,
            KEY_l = SDL.SDL_Keycode.SDLK_l,
            KEY_m = SDL.SDL_Keycode.SDLK_m,
            KEY_n = SDL.SDL_Keycode.SDLK_n,
            KEY_o = SDL.SDL_Keycode.SDLK_o,
            KEY_p = SDL.SDL_Keycode.SDLK_p,
            KEY_q = SDL.SDL_Keycode.SDLK_q,
            KEY_r = SDL.SDL_Keycode.SDLK_r,
            KEY_s = SDL.SDL_Keycode.SDLK_s,
            KEY_t = SDL.SDL_Keycode.SDLK_t,
            KEY_u = SDL.SDL_Keycode.SDLK_u,
            KEY_v = SDL.SDL_Keycode.SDLK_v,
            KEY_w = SDL.SDL_Keycode.SDLK_w,
            KEY_x = SDL.SDL_Keycode.SDLK_x,
            KEY_y = SDL.SDL_Keycode.SDLK_y,
            KEY_z = SDL.SDL_Keycode.SDLK_z,
            KEY_DELETE = SDL.SDL_Keycode.SDLK_DELETE,

            KEY_KP0 = SDL.SDL_Keycode.SDLK_KP_0,
            KEY_KP1 = SDL.SDL_Keycode.SDLK_KP_1,
            KEY_KP2 = SDL.SDL_Keycode.SDLK_KP_2,
            KEY_KP3 = SDL.SDL_Keycode.SDLK_KP_3,
            KEY_KP4 = SDL.SDL_Keycode.SDLK_KP_4,
            KEY_KP5 = SDL.SDL_Keycode.SDLK_KP_5,
            KEY_KP6 = SDL.SDL_Keycode.SDLK_KP_6,
            KEY_KP7 = SDL.SDL_Keycode.SDLK_KP_7,
            KEY_KP8 = SDL.SDL_Keycode.SDLK_KP_8,
            KEY_KP9 = SDL.SDL_Keycode.SDLK_KP_9,
            KEY_KP_PERIOD = SDL.SDL_Keycode.SDLK_KP_PERIOD,
            KEY_KP_DIVIDE = SDL.SDL_Keycode.SDLK_KP_DIVIDE,
            KEY_KP_MULTIPLY = SDL.SDL_Keycode.SDLK_KP_MULTIPLY,
            KEY_KP_MINUS = SDL.SDL_Keycode.SDLK_KP_MINUS,
            KEY_KP_PLUS = SDL.SDL_Keycode.SDLK_KP_PLUS,
            KEY_KP_ENTER = SDL.SDL_Keycode.SDLK_KP_ENTER,
            KEY_KP_EQUALS = SDL.SDL_Keycode.SDLK_KP_EQUALS,

            KEY_UP = SDL.SDL_Keycode.SDLK_UP,
            KEY_DOWN = SDL.SDL_Keycode.SDLK_DOWN,
            KEY_RIGHT = SDL.SDL_Keycode.SDLK_RIGHT,
            KEY_LEFT = SDL.SDL_Keycode.SDLK_LEFT,
            KEY_INSERT = SDL.SDL_Keycode.SDLK_INSERT,
            KEY_HOME = SDL.SDL_Keycode.SDLK_HOME,
            KEY_END = SDL.SDL_Keycode.SDLK_END,
            KEY_PAGE_UP = SDL.SDL_Keycode.SDLK_PAGEUP,
            KEY_PAGE_DOWN = SDL.SDL_Keycode.SDLK_PAGEDOWN,

            KEY_F1 = SDL.SDL_Keycode.SDLK_F1,
            KEY_F2 = SDL.SDL_Keycode.SDLK_F2,
            KEY_F3 = SDL.SDL_Keycode.SDLK_F3,
            KEY_F4 = SDL.SDL_Keycode.SDLK_F4,
            KEY_F5 = SDL.SDL_Keycode.SDLK_F5,
            KEY_F6 = SDL.SDL_Keycode.SDLK_F6,
            KEY_F7 = SDL.SDL_Keycode.SDLK_F7,
            KEY_F8 = SDL.SDL_Keycode.SDLK_F8,
            KEY_F9 = SDL.SDL_Keycode.SDLK_F9,
            KEY_F10 = SDL.SDL_Keycode.SDLK_F10,
            KEY_F11 = SDL.SDL_Keycode.SDLK_F11,
            KEY_F12 = SDL.SDL_Keycode.SDLK_F12,
            KEY_F13 = SDL.SDL_Keycode.SDLK_F13,
            KEY_F14 = SDL.SDL_Keycode.SDLK_F14,
            KEY_F15 = SDL.SDL_Keycode.SDLK_F15,

            KEY_NUMLOCK = SDL.SDL_Keycode.SDLK_NUMLOCKCLEAR,
            KEY_CAPSLOCK = SDL.SDL_Keycode.SDLK_CAPSLOCK,
            KEY_SCROLLOCK = SDL.SDL_Keycode.SDLK_SCROLLLOCK,
            KEY_RIGHT_SHIFT = SDL.SDL_Keycode.SDLK_RSHIFT,
            KEY_LEFT_SHIFT = SDL.SDL_Keycode.SDLK_LSHIFT,
            KEY_RIGHT_CONTROL = SDL.SDL_Keycode.SDLK_RCTRL,
            KEY_LEFT_CONTROL = SDL.SDL_Keycode.SDLK_LCTRL,
            KEY_RIGHT_ALT = SDL.SDL_Keycode.SDLK_RALT,
            KEY_LEFT_ALT = SDL.SDL_Keycode.SDLK_LALT,
            KEY_RIGHT_META = SDL.SDL_Keycode.SDLK_RGUI,
            KEY_LEFT_META = SDL.SDL_Keycode.SDLK_LGUI
        }

        public static void Initialize()
        {
            SDL.SDL_StartTextInput();
        }

        public static void Quit()
        {
            SDL.SDL_StopTextInput();
        }

        public static Input.Event GetEvent()
        {
            SDL.SDL_Event internalEvent;
            if (SDL.SDL_PollEvent(out internalEvent) == 0)
                return null;
            
            return new Event(internalEvent);
        }
    }
}
