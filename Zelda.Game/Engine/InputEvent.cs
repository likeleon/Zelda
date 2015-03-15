using SDL2;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zelda.Game.Engine
{
    class InputEvent
    {
        public enum KeyboardKeys
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

        public bool IsWindowClosing
        {
            get { return _internalEvent.type == SDL.SDL_EventType.SDL_QUIT; }
        }

        [Description("KEYDOWN과 KEYUP의 이벤트들의 반복을 처리하려면 true로 설정합니다")]
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

        public KeyboardKeys KeyboardKey
        {
            get
            {
                if (!IsKeyboardEvent)
                    return KeyboardKeys.KEY_NONE;

                return (KeyboardKeys)_internalEvent.key.keysym.sym;
            }
        }

        static readonly Dictionary<KeyboardKeys, string> KeyboardKeyNames
            = new Dictionary<KeyboardKeys, string>()
        {
            { KeyboardKeys.KEY_NONE,              "" },
            { KeyboardKeys.KEY_BACKSPACE,         "backspace" },
            { KeyboardKeys.KEY_TABULATION,        "tab" },
            { KeyboardKeys.KEY_CLEAR,             "clear" },
            { KeyboardKeys.KEY_RETURN,            "return" },
            { KeyboardKeys.KEY_PAUSE,             "pause" },
            { KeyboardKeys.KEY_ESCAPE,            "escape" },
            { KeyboardKeys.KEY_SPACE,             "space" },
            { KeyboardKeys.KEY_EXCLAMATION_MARK,  "!" },
            { KeyboardKeys.KEY_DOULE_QUOTE,       "\"" },
            { KeyboardKeys.KEY_HASH,              "#" },
            { KeyboardKeys.KEY_DOLLAR,            "$" },
            { KeyboardKeys.KEY_AMPERSAND,         "&" },
            { KeyboardKeys.KEY_SINGLE_QUOTE,      "'" },
            { KeyboardKeys.KEY_LEFT_PARENTHESIS,  "(" },
            { KeyboardKeys.KEY_RIGHT_PARENTHESIS, ")" },
            { KeyboardKeys.KEY_ASTERISK,          "*" },
            { KeyboardKeys.KEY_PLUS,              "+" },
            { KeyboardKeys.KEY_COMMA,             "," },
            { KeyboardKeys.KEY_MINUS,             "-" },
            { KeyboardKeys.KEY_PERIOD,            "." },
            { KeyboardKeys.KEY_SLASH,             "/" },
            { KeyboardKeys.KEY_0,                 "0" },
            { KeyboardKeys.KEY_1,                 "1" },
            { KeyboardKeys.KEY_2,                 "2" },
            { KeyboardKeys.KEY_3,                 "3" },
            { KeyboardKeys.KEY_4,                 "4" },
            { KeyboardKeys.KEY_5,                 "5" },
            { KeyboardKeys.KEY_6,                 "6" },
            { KeyboardKeys.KEY_7,                 "7" },
            { KeyboardKeys.KEY_8,                 "8" },
            { KeyboardKeys.KEY_9,                 "9" },
            { KeyboardKeys.KEY_COLON,             "." },
            { KeyboardKeys.KEY_SEMICOLON,         ":" },
            { KeyboardKeys.KEY_LESS,              "<" },
            { KeyboardKeys.KEY_EQUALS,            "=" },
            { KeyboardKeys.KEY_GREATER,           ">" },
            { KeyboardKeys.KEY_QUESTION_MARK,     "?" },
            { KeyboardKeys.KEY_AT,                "@" },
            { KeyboardKeys.KEY_LEFT_BRACKET,      "[" },
            { KeyboardKeys.KEY_BACKSLASH,         "\\" },
            { KeyboardKeys.KEY_RIGHT_BRACKET,     "]" },
            { KeyboardKeys.KEY_CARET,             "^" },
            { KeyboardKeys.KEY_UNDERSCORE,        "_" },
            { KeyboardKeys.KEY_BACKQUOTE,         "`" },
            { KeyboardKeys.KEY_a,                 "a" },
            { KeyboardKeys.KEY_b,                 "b" },
            { KeyboardKeys.KEY_c,                 "c" },
            { KeyboardKeys.KEY_d,                 "d" },
            { KeyboardKeys.KEY_e,                 "e" },
            { KeyboardKeys.KEY_f,                 "f" },
            { KeyboardKeys.KEY_g,                 "g" },
            { KeyboardKeys.KEY_h,                 "h" },
            { KeyboardKeys.KEY_i,                 "i" },
            { KeyboardKeys.KEY_j,                 "j" },
            { KeyboardKeys.KEY_k,                 "k" },
            { KeyboardKeys.KEY_l,                 "l" },
            { KeyboardKeys.KEY_m,                 "m" },
            { KeyboardKeys.KEY_n,                 "n" },
            { KeyboardKeys.KEY_o,                 "o" },
            { KeyboardKeys.KEY_p,                 "p" },
            { KeyboardKeys.KEY_q,                 "q" },
            { KeyboardKeys.KEY_r,                 "r" },
            { KeyboardKeys.KEY_s,                 "s" },
            { KeyboardKeys.KEY_t,                 "t" },
            { KeyboardKeys.KEY_u,                 "u" },
            { KeyboardKeys.KEY_v,                 "v" },
            { KeyboardKeys.KEY_w,                 "w" },
            { KeyboardKeys.KEY_x,                 "x" },
            { KeyboardKeys.KEY_y,                 "y" },
            { KeyboardKeys.KEY_z,                 "z" },
            { KeyboardKeys.KEY_DELETE,            "delete" },
            { KeyboardKeys.KEY_KP0,               "kp 0" },
            { KeyboardKeys.KEY_KP1,               "kp 1" },
            { KeyboardKeys.KEY_KP2,               "kp 2" },
            { KeyboardKeys.KEY_KP3,               "kp 3" },
            { KeyboardKeys.KEY_KP4,               "kp 4" },
            { KeyboardKeys.KEY_KP5,               "kp 5" },
            { KeyboardKeys.KEY_KP6,               "kp 6" },
            { KeyboardKeys.KEY_KP7,               "kp 7" },
            { KeyboardKeys.KEY_KP8,               "kp 8" },
            { KeyboardKeys.KEY_KP9,               "kp 9" },
            { KeyboardKeys.KEY_KP_PERIOD,         "kp ." },
            { KeyboardKeys.KEY_KP_DIVIDE,         "kp /" },
            { KeyboardKeys.KEY_KP_MULTIPLY,       "kp *" },
            { KeyboardKeys.KEY_KP_MINUS,          "kp -" },
            { KeyboardKeys.KEY_KP_PLUS,           "kp +" },
            { KeyboardKeys.KEY_KP_ENTER,          "kp return" },
            { KeyboardKeys.KEY_KP_EQUALS,         "kp =" },
            { KeyboardKeys.KEY_UP,                "up" },
            { KeyboardKeys.KEY_DOWN,              "down" },
            { KeyboardKeys.KEY_RIGHT,             "right" },
            { KeyboardKeys.KEY_LEFT,              "left" },
            { KeyboardKeys.KEY_INSERT,            "insert" },
            { KeyboardKeys.KEY_HOME,              "home" },
            { KeyboardKeys.KEY_END,               "end" },
            { KeyboardKeys.KEY_PAGE_UP,           "page up" },
            { KeyboardKeys.KEY_PAGE_DOWN,         "page down" },
            { KeyboardKeys.KEY_F1,                "f1" },
            { KeyboardKeys.KEY_F2,                "f2" },
            { KeyboardKeys.KEY_F3,                "f3" },
            { KeyboardKeys.KEY_F4,                "f4" },
            { KeyboardKeys.KEY_F5,                "f5" },
            { KeyboardKeys.KEY_F6,                "f6" },
            { KeyboardKeys.KEY_F7,                "f7" },
            { KeyboardKeys.KEY_F8,                "f8" },
            { KeyboardKeys.KEY_F9,                "f9" },
            { KeyboardKeys.KEY_F10,               "f10" },
            { KeyboardKeys.KEY_F11,               "f11" },
            { KeyboardKeys.KEY_F12,               "f12" },
            { KeyboardKeys.KEY_F13,               "f13" },
            { KeyboardKeys.KEY_F14,               "f14" },
            { KeyboardKeys.KEY_F15,               "f15" },
            { KeyboardKeys.KEY_NUMLOCK,           "num lock" },
            { KeyboardKeys.KEY_CAPSLOCK,          "caps lock" },
            { KeyboardKeys.KEY_SCROLLOCK,         "scroll lock" },
            { KeyboardKeys.KEY_RIGHT_SHIFT,       "right shift" },
            { KeyboardKeys.KEY_LEFT_SHIFT,        "left shift" },
            { KeyboardKeys.KEY_RIGHT_CONTROL,     "right control" },
            { KeyboardKeys.KEY_LEFT_CONTROL,      "left control" },
            { KeyboardKeys.KEY_RIGHT_ALT,         "right alt" },
            { KeyboardKeys.KEY_LEFT_ALT,          "left alt" },
            { KeyboardKeys.KEY_RIGHT_META,        "right meta" },
            { KeyboardKeys.KEY_LEFT_META,         "left meta" }
        };

        readonly SDL.SDL_Event _internalEvent;

        internal InputEvent(SDL.SDL_Event sdlEvent)
        {
            _internalEvent = sdlEvent;
        }

        public static void Initialize()
        {
            SDL.SDL_StartTextInput();
        }

        public static void Quit()
        {
            SDL.SDL_StopTextInput();
        }

        public static InputEvent GetEvent()
        {
            SDL.SDL_Event internalEvent;
            if (SDL.SDL_PollEvent(out internalEvent) == 0)
                return null;
            
            return new InputEvent(internalEvent);
        }

        public static string GetKeyboardKeyName(KeyboardKeys key)
        {
            return KeyboardKeyNames[key];
        }
    }
}
