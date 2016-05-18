using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Zelda.Game.LowLevel
{
    class FontResource
    {
        class OutlineFontReader : DisposableObject
        {
            readonly IntPtr _rw;
            readonly IntPtr _outlineFont;

            public IntPtr OutlineFont { get { return _outlineFont; } }

            public OutlineFontReader(IntPtr rw, IntPtr outlineFont)
            {
                _rw = rw;
                _outlineFont = outlineFont;
            }

            protected override void OnDispose(bool disposing)
            {
                SDL_ttf.TTF_CloseFont(_outlineFont);
                // TODO: 네이티브의 SDL_RWclose(rw)에 해당하는 구현 방법을 찾지 못했다. _rw에 대한 메모리 릭 확인할 것.
            }
        }

        class FontFile : DisposableObject
        {
            readonly string _fileName;
            readonly byte[] _buffer;
            readonly GCHandle _bufferHandle;

            public string FileName { get { return _fileName; } }
            public int BufferLength { get { return _buffer.Length; } }
            public Surface BitmapFont { get; set; }
            public Dictionary<int, OutlineFontReader> OutlineFonts { get; private set; }

            public FontFile(string fileName, bool is_bitmap)
            {
                _fileName = fileName;

                if (is_bitmap)
                    BitmapFont = Surface.Create(_fileName, Surface.ImageDirectory.Data);
                else
                {
                    _buffer = MainLoop.CurrentMod.ModFiles.DataFileRead(_fileName);
                    _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                    OutlineFonts = new Dictionary<int, OutlineFontReader>();
                }
            }
            
            public IntPtr GetBufferPtr()
            {
                return _bufferHandle.AddrOfPinnedObject(); 
            }

            protected override void OnDispose(bool disposing)
            {
                if (OutlineFonts != null)
                {
                    foreach (var font in OutlineFonts.Values)
                        font.Dispose();
                }

                if (_bufferHandle.IsAllocated)
                    _bufferHandle.Free();
            }
        }

        static bool _fontsLoaded;
        static readonly Dictionary<string, FontFile> _fonts = new Dictionary<string, FontFile>();

        public static void Initialize()
        {
            SDL_ttf.TTF_Init();
        }

        public static void Quit()
        {
            foreach (var font in _fonts.Values)
                font.Dispose();

            _fonts.Clear();
            _fontsLoaded = false;
            SDL_ttf.TTF_Quit();
        }

        public static string GetDefaultFontId()
        {
            if (!_fontsLoaded)
                LoadFonts();

            if (_fonts.Count <= 0)
                return String.Empty;

            return _fonts.First().Key;
        }

        static void LoadFonts()
        {
            foreach (var kvp in MainLoop.CurrentMod.GetResources(ResourceType.Font))
            {
                string fontId = kvp.Key;
                
                var fontExts = new[]
                {
                    new { ext = ".png", bitmap = true },
                    new { ext = ".PNG", bitmap = true },
                    new { ext = ".ttf", bitmap = false },
                    new { ext = ".TTF", bitmap = false },
                    new { ext = ".ttc", bitmap = false },
                    new { ext = ".TTC", bitmap = false },
                    new { ext = ".fon", bitmap = false },
                    new { ext = ".FON", bitmap = false },
                };
                
                string fileNameStart = "fonts/" + fontId;
                var fontExt = fontExts.FirstOrDefault(e => MainLoop.CurrentMod.ModFiles.DataFileExists(fileNameStart + e.ext));
                if (fontExt == null)
                {
                    Debug.Error("Cannot find font file 'fonts/{0}' (tried with extensions {1}"
                        .F(fontId, String.Join(", ", fontExts.Select(f => f.ext))));
                    continue;
                }

                var font = new FontFile(fileNameStart + fontExt.ext, fontExt.bitmap);
                _fonts.Add(fontId, font);
            }

            _fontsLoaded = true;
        }

        public static bool Exists(string fontId)
        {
            if (!_fontsLoaded)
                LoadFonts();

            return _fonts.ContainsKey(fontId);
        }

        public static bool IsBitmapFont(string fontId)
        {
            if (!_fontsLoaded)
                LoadFonts();

            Debug.CheckAssertion(Exists(fontId), "No such font: '{0}'".F(fontId));
            return (_fonts[fontId].BitmapFont != null);
        }

        public static Surface GetBitmapFont(string fontId)
        {
            if (!_fontsLoaded)
                LoadFonts();

            Debug.CheckAssertion(Exists(fontId), "No such font: '{0}'".F(fontId));
            Debug.CheckAssertion(IsBitmapFont(fontId), "This is not a bitmap font: '{0}'".F(fontId));
            return (_fonts[fontId].BitmapFont);
        }

        public static IntPtr GetOutlineFont(string fontId, int size)
        {
            if (!_fontsLoaded)
                LoadFonts();

            Debug.CheckAssertion(Exists(fontId), "No such font: '{0}'".F(fontId));
            Debug.CheckAssertion(!IsBitmapFont(fontId), "This is not a outline font: '{0}'".F(fontId));

            var font = _fonts[fontId];
            var outlineFonts = font.OutlineFonts;
            if (outlineFonts.ContainsKey(size))
                return outlineFonts[size].OutlineFont;

            var rw = SDL.SDL_RWFromMem(font.GetBufferPtr(), font.BufferLength);
            var outlineFont = SDL_ttf.TTF_OpenFontRW(rw, 0, size);
            Debug.CheckAssertion(outlineFont != IntPtr.Zero,
                "Cannot load font from file '{0}': {1}".F(font.FileName, SDL.SDL_GetError()));
            var reader = new OutlineFontReader(rw, outlineFont);
            outlineFonts.Add(size, reader);
            return outlineFonts[size].OutlineFont;
        }
    }
}
