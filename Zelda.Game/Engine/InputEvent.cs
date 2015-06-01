using SDL2;
using System.Collections.Generic;
using System.ComponentModel;

namespace Zelda.Game.Engine
{
    class InputEvent
    {
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

        public KeyboardKey KeyboardKey
        {
            get
            {
                if (!IsKeyboardEvent)
                    return KeyboardKey.KEY_NONE;

                return (KeyboardKey)_internalEvent.key.keysym.sym;
            }
        }

        static readonly Dictionary<KeyboardKey, string> _keyboardKeyNames = new Dictionary<KeyboardKey, string>()
        {
            { KeyboardKey.KEY_NONE,              "" },
            { KeyboardKey.KEY_BACKSPACE,         "backspace" },
            { KeyboardKey.KEY_TABULATION,        "tab" },
            { KeyboardKey.KEY_CLEAR,             "clear" },
            { KeyboardKey.KEY_RETURN,            "return" },
            { KeyboardKey.KEY_PAUSE,             "pause" },
            { KeyboardKey.KEY_ESCAPE,            "escape" },
            { KeyboardKey.KEY_SPACE,             "space" },
            { KeyboardKey.KEY_EXCLAMATION_MARK,  "!" },
            { KeyboardKey.KEY_DOULE_QUOTE,       "\"" },
            { KeyboardKey.KEY_HASH,              "#" },
            { KeyboardKey.KEY_DOLLAR,            "$" },
            { KeyboardKey.KEY_AMPERSAND,         "&" },
            { KeyboardKey.KEY_SINGLE_QUOTE,      "'" },
            { KeyboardKey.KEY_LEFT_PARENTHESIS,  "(" },
            { KeyboardKey.KEY_RIGHT_PARENTHESIS, ")" },
            { KeyboardKey.KEY_ASTERISK,          "*" },
            { KeyboardKey.KEY_PLUS,              "+" },
            { KeyboardKey.KEY_COMMA,             "," },
            { KeyboardKey.KEY_MINUS,             "-" },
            { KeyboardKey.KEY_PERIOD,            "." },
            { KeyboardKey.KEY_SLASH,             "/" },
            { KeyboardKey.KEY_0,                 "0" },
            { KeyboardKey.KEY_1,                 "1" },
            { KeyboardKey.KEY_2,                 "2" },
            { KeyboardKey.KEY_3,                 "3" },
            { KeyboardKey.KEY_4,                 "4" },
            { KeyboardKey.KEY_5,                 "5" },
            { KeyboardKey.KEY_6,                 "6" },
            { KeyboardKey.KEY_7,                 "7" },
            { KeyboardKey.KEY_8,                 "8" },
            { KeyboardKey.KEY_9,                 "9" },
            { KeyboardKey.KEY_COLON,             "." },
            { KeyboardKey.KEY_SEMICOLON,         ":" },
            { KeyboardKey.KEY_LESS,              "<" },
            { KeyboardKey.KEY_EQUALS,            "=" },
            { KeyboardKey.KEY_GREATER,           ">" },
            { KeyboardKey.KEY_QUESTION_MARK,     "?" },
            { KeyboardKey.KEY_AT,                "@" },
            { KeyboardKey.KEY_LEFT_BRACKET,      "[" },
            { KeyboardKey.KEY_BACKSLASH,         "\\" },
            { KeyboardKey.KEY_RIGHT_BRACKET,     "]" },
            { KeyboardKey.KEY_CARET,             "^" },
            { KeyboardKey.KEY_UNDERSCORE,        "_" },
            { KeyboardKey.KEY_BACKQUOTE,         "`" },
            { KeyboardKey.KEY_a,                 "a" },
            { KeyboardKey.KEY_b,                 "b" },
            { KeyboardKey.KEY_c,                 "c" },
            { KeyboardKey.KEY_d,                 "d" },
            { KeyboardKey.KEY_e,                 "e" },
            { KeyboardKey.KEY_f,                 "f" },
            { KeyboardKey.KEY_g,                 "g" },
            { KeyboardKey.KEY_h,                 "h" },
            { KeyboardKey.KEY_i,                 "i" },
            { KeyboardKey.KEY_j,                 "j" },
            { KeyboardKey.KEY_k,                 "k" },
            { KeyboardKey.KEY_l,                 "l" },
            { KeyboardKey.KEY_m,                 "m" },
            { KeyboardKey.KEY_n,                 "n" },
            { KeyboardKey.KEY_o,                 "o" },
            { KeyboardKey.KEY_p,                 "p" },
            { KeyboardKey.KEY_q,                 "q" },
            { KeyboardKey.KEY_r,                 "r" },
            { KeyboardKey.KEY_s,                 "s" },
            { KeyboardKey.KEY_t,                 "t" },
            { KeyboardKey.KEY_u,                 "u" },
            { KeyboardKey.KEY_v,                 "v" },
            { KeyboardKey.KEY_w,                 "w" },
            { KeyboardKey.KEY_x,                 "x" },
            { KeyboardKey.KEY_y,                 "y" },
            { KeyboardKey.KEY_z,                 "z" },
            { KeyboardKey.KEY_DELETE,            "delete" },
            { KeyboardKey.KEY_KP0,               "kp 0" },
            { KeyboardKey.KEY_KP1,               "kp 1" },
            { KeyboardKey.KEY_KP2,               "kp 2" },
            { KeyboardKey.KEY_KP3,               "kp 3" },
            { KeyboardKey.KEY_KP4,               "kp 4" },
            { KeyboardKey.KEY_KP5,               "kp 5" },
            { KeyboardKey.KEY_KP6,               "kp 6" },
            { KeyboardKey.KEY_KP7,               "kp 7" },
            { KeyboardKey.KEY_KP8,               "kp 8" },
            { KeyboardKey.KEY_KP9,               "kp 9" },
            { KeyboardKey.KEY_KP_PERIOD,         "kp ." },
            { KeyboardKey.KEY_KP_DIVIDE,         "kp /" },
            { KeyboardKey.KEY_KP_MULTIPLY,       "kp *" },
            { KeyboardKey.KEY_KP_MINUS,          "kp -" },
            { KeyboardKey.KEY_KP_PLUS,           "kp +" },
            { KeyboardKey.KEY_KP_ENTER,          "kp return" },
            { KeyboardKey.KEY_KP_EQUALS,         "kp =" },
            { KeyboardKey.KEY_UP,                "up" },
            { KeyboardKey.KEY_DOWN,              "down" },
            { KeyboardKey.KEY_RIGHT,             "right" },
            { KeyboardKey.KEY_LEFT,              "left" },
            { KeyboardKey.KEY_INSERT,            "insert" },
            { KeyboardKey.KEY_HOME,              "home" },
            { KeyboardKey.KEY_END,               "end" },
            { KeyboardKey.KEY_PAGE_UP,           "page up" },
            { KeyboardKey.KEY_PAGE_DOWN,         "page down" },
            { KeyboardKey.KEY_F1,                "f1" },
            { KeyboardKey.KEY_F2,                "f2" },
            { KeyboardKey.KEY_F3,                "f3" },
            { KeyboardKey.KEY_F4,                "f4" },
            { KeyboardKey.KEY_F5,                "f5" },
            { KeyboardKey.KEY_F6,                "f6" },
            { KeyboardKey.KEY_F7,                "f7" },
            { KeyboardKey.KEY_F8,                "f8" },
            { KeyboardKey.KEY_F9,                "f9" },
            { KeyboardKey.KEY_F10,               "f10" },
            { KeyboardKey.KEY_F11,               "f11" },
            { KeyboardKey.KEY_F12,               "f12" },
            { KeyboardKey.KEY_F13,               "f13" },
            { KeyboardKey.KEY_F14,               "f14" },
            { KeyboardKey.KEY_F15,               "f15" },
            { KeyboardKey.KEY_NUMLOCK,           "num lock" },
            { KeyboardKey.KEY_CAPSLOCK,          "caps lock" },
            { KeyboardKey.KEY_SCROLLOCK,         "scroll lock" },
            { KeyboardKey.KEY_RIGHT_SHIFT,       "right shift" },
            { KeyboardKey.KEY_LEFT_SHIFT,        "left shift" },
            { KeyboardKey.KEY_RIGHT_CONTROL,     "right control" },
            { KeyboardKey.KEY_LEFT_CONTROL,      "left control" },
            { KeyboardKey.KEY_RIGHT_ALT,         "right alt" },
            { KeyboardKey.KEY_LEFT_ALT,          "left alt" },
            { KeyboardKey.KEY_RIGHT_META,        "right meta" },
            { KeyboardKey.KEY_LEFT_META,         "left meta" }
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

        public static string GetKeyboardKeyName(KeyboardKey key)
        {
            return _keyboardKeyNames[key];
        }

        public static KeyboardKey GetKeyboardKeyByName(string keyboardKeyName)
        {
            // TODO: 성능이 문제가 된다면 역참조 매핑을 만들어야 합니다.
            foreach (var kv in _keyboardKeyNames)
            {
                if (kv.Value == keyboardKeyName)
                    return kv.Key;
            }
            return KeyboardKey.KEY_NONE;
        }
    }
}
