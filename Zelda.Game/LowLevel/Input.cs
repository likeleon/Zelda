﻿using SDL2;
using System;
using System.Collections.Generic;

namespace Zelda.Game.LowLevel
{
    public class Input : IDisposable
    {
        internal IReadOnlyDictionary<KeyboardKey, string> KeyNames { get; } = new Dictionary<KeyboardKey, string>()
        {
            { KeyboardKey.None,              "" },
            { KeyboardKey.Backspace,         "backspace" },
            { KeyboardKey.Tabulation,        "tab" },
            { KeyboardKey.Clear,             "clear" },
            { KeyboardKey.Return,            "return" },
            { KeyboardKey.Pause,             "pause" },
            { KeyboardKey.Escape,            "escape" },
            { KeyboardKey.Space,             "space" },
            { KeyboardKey.ExclamationMark,   "!" },
            { KeyboardKey.DoubleQuote,       "\"" },
            { KeyboardKey.Hash,              "#" },
            { KeyboardKey.Dollar,            "$" },
            { KeyboardKey.Ampersand,         "&" },
            { KeyboardKey.SingleQuote,       "'" },
            { KeyboardKey.LeftParenthesis,   "(" },
            { KeyboardKey.RightParenthesis,  ")" },
            { KeyboardKey.Aterisk,           "*" },
            { KeyboardKey.Plus,              "+" },
            { KeyboardKey.Comma,             "," },
            { KeyboardKey.Minus,             "-" },
            { KeyboardKey.Period,            "." },
            { KeyboardKey.Slash,             "/" },
            { KeyboardKey.Zero,              "0" },
            { KeyboardKey.One,               "1" },
            { KeyboardKey.Two,               "2" },
            { KeyboardKey.Three,             "3" },
            { KeyboardKey.Four,              "4" },
            { KeyboardKey.Five,              "5" },
            { KeyboardKey.Six,               "6" },
            { KeyboardKey.Seven,             "7" },
            { KeyboardKey.Eight,             "8" },
            { KeyboardKey.Nine,               "9" },
            { KeyboardKey.Colon,             "." },
            { KeyboardKey.Semicolon,         ":" },
            { KeyboardKey.Less,              "<" },
            { KeyboardKey.Equal,             "=" },
            { KeyboardKey.Greater,           ">" },
            { KeyboardKey.QuestionMark,      "?" },
            { KeyboardKey.At,                "@" },
            { KeyboardKey.LeftBracket,       "[" },
            { KeyboardKey.Backslash,         "\\" },
            { KeyboardKey.RightBracket,      "]" },
            { KeyboardKey.Caret,             "^" },
            { KeyboardKey.Underscore,        "_" },
            { KeyboardKey.Backquote,         "`" },
            { KeyboardKey.A,                 "a" },
            { KeyboardKey.B,                 "b" },
            { KeyboardKey.C,                 "c" },
            { KeyboardKey.D,                 "d" },
            { KeyboardKey.E,                 "e" },
            { KeyboardKey.F,                 "f" },
            { KeyboardKey.G,                 "g" },
            { KeyboardKey.H,                 "h" },
            { KeyboardKey.I,                 "i" },
            { KeyboardKey.J,                 "j" },
            { KeyboardKey.K,                 "k" },
            { KeyboardKey.L,                 "l" },
            { KeyboardKey.M,                 "m" },
            { KeyboardKey.N,                 "n" },
            { KeyboardKey.O,                 "o" },
            { KeyboardKey.P,                 "p" },
            { KeyboardKey.Q,                 "q" },
            { KeyboardKey.R,                 "r" },
            { KeyboardKey.S,                 "s" },
            { KeyboardKey.T,                 "t" },
            { KeyboardKey.U,                 "u" },
            { KeyboardKey.V,                 "v" },
            { KeyboardKey.W,                 "w" },
            { KeyboardKey.X,                 "x" },
            { KeyboardKey.Y,                 "y" },
            { KeyboardKey.Z,                 "z" },
            { KeyboardKey.Delete,            "delete" },
            { KeyboardKey.Kp0,               "kp 0" },
            { KeyboardKey.Kp1,               "kp 1" },
            { KeyboardKey.Kp2,               "kp 2" },
            { KeyboardKey.Kp3,               "kp 3" },
            { KeyboardKey.Kp4,               "kp 4" },
            { KeyboardKey.Kp5,               "kp 5" },
            { KeyboardKey.Kp6,               "kp 6" },
            { KeyboardKey.Kp7,               "kp 7" },
            { KeyboardKey.Kp8,               "kp 8" },
            { KeyboardKey.Kp9,               "kp 9" },
            { KeyboardKey.KpPeriod,          "kp ." },
            { KeyboardKey.KpDivide,          "kp /" },
            { KeyboardKey.KpMultiply,        "kp *" },
            { KeyboardKey.KpMinus,           "kp -" },
            { KeyboardKey.KpPlus,            "kp +" },
            { KeyboardKey.KpEnter,           "kp return" },
            { KeyboardKey.KpEqual,           "kp =" },
            { KeyboardKey.Up,                "up" },
            { KeyboardKey.Down,              "down" },
            { KeyboardKey.Right,             "right" },
            { KeyboardKey.Left,              "left" },
            { KeyboardKey.Insert,            "insert" },
            { KeyboardKey.Home,              "home" },
            { KeyboardKey.End,               "end" },
            { KeyboardKey.PageUp,            "page up" },
            { KeyboardKey.PageDown,          "page down" },
            { KeyboardKey.F1,                "f1" },
            { KeyboardKey.F2,                "f2" },
            { KeyboardKey.F3,                "f3" },
            { KeyboardKey.F4,                "f4" },
            { KeyboardKey.F5,                "f5" },
            { KeyboardKey.F6,                "f6" },
            { KeyboardKey.F7,                "f7" },
            { KeyboardKey.F8,                "f8" },
            { KeyboardKey.F9,                "f9" },
            { KeyboardKey.F10,               "f10" },
            { KeyboardKey.F11,               "f11" },
            { KeyboardKey.F12,               "f12" },
            { KeyboardKey.F13,               "f13" },
            { KeyboardKey.F14,               "f14" },
            { KeyboardKey.F15,               "f15" },
            { KeyboardKey.NumLock,           "num lock" },
            { KeyboardKey.CapsLock,          "caps lock" },
            { KeyboardKey.ScrollLock,        "scroll lock" },
            { KeyboardKey.RightShift,        "right shift" },
            { KeyboardKey.LeftShift,         "left shift" },
            { KeyboardKey.RightControl,      "right control" },
            { KeyboardKey.LeftControl,       "left control" },
            { KeyboardKey.RightAlt,          "right alt" },
            { KeyboardKey.LeftAlt,           "left alt" },
            { KeyboardKey.RightMeta,         "right meta" },
            { KeyboardKey.LeftMeta,          "left meta" }
        };

        internal Input()
        {
            SDL.SDL_StartTextInput();
        }

        public void Dispose()
        {
            SDL.SDL_StopTextInput();
        }

        internal InputEvent GetEvent()
        {
            SDL.SDL_Event internalEvent;
            if (SDL.SDL_PollEvent(out internalEvent) == 0)
                return null;

            return new InputEvent(internalEvent);
        }

        internal KeyboardKey GetKeyboardKeyByName(string name)
        {
            // TODO: 성능이 문제가 된다면 역참조 매핑을 만들어야 합니다.
            foreach (var kv in KeyNames)
            {
                if (kv.Value == name)
                    return kv.Key;
            }
            return KeyboardKey.None;
        }

    }
}
