using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Zelda.Game.LowLevel
{
    class FontResource : IDisposable
    {
        class OutlineFontReader : IDisposable
        {
            public IntPtr OutlineFont { get; }

            readonly IntPtr _rw;

            public OutlineFontReader(IntPtr rw, IntPtr outlineFont)
            {
                _rw = rw;
                OutlineFont = outlineFont;
            }

            public void Dispose()
            {
                SDL_ttf.TTF_CloseFont(OutlineFont);
                // TODO: 네이티브의 SDL_RWclose(rw)에 해당하는 구현 방법을 찾지 못했다. _rw에 대한 메모리 릭 확인할 것.
            }
        }

        class FontFile : IDisposable
        {
            public string FileName { get; }
            public int BufferLength => _buffer.Length;
            public Surface BitmapFont { get; }
            public Dictionary<int, OutlineFontReader> OutlineFonts { get; }

            readonly byte[] _buffer;
            readonly GCHandle _bufferHandle;

            public FontFile(string fileName, bool is_bitmap)
            {
                FileName = fileName;

                if (is_bitmap)
                    BitmapFont = Surface.Create(FileName, Surface.ImageDirectory.Data);
                else
                {
                    _buffer = Core.Mod.ModFiles.DataFileRead(FileName);
                    _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                    OutlineFonts = new Dictionary<int, OutlineFontReader>();
                }
            }

            public void Dispose()
            {
                if (OutlineFonts != null)
                {
                    foreach (var font in OutlineFonts.Values)
                        font.Dispose();
                }

                if (_bufferHandle.IsAllocated)
                    _bufferHandle.Free();
            }
            
            public IntPtr GetBufferPtr()
            {
                return _bufferHandle.AddrOfPinnedObject(); 
            }
        }

        readonly Dictionary<string, FontFile> _fonts = new Dictionary<string, FontFile>();
        bool _fontsLoaded;

        public FontResource()
        {
            SDL_ttf.TTF_Init();
        }

        public void Dispose()
        {
            _fonts.Values.Do(f => f.Dispose());
            SDL_ttf.TTF_Quit();
        }

        public string GetDefaultFontId()
        {
            if (!_fontsLoaded)
                LoadFonts();

            if (_fonts.Count <= 0)
                return null;

            return _fonts.First().Key;
        }

        void LoadFonts()
        {
            foreach (var kvp in Core.Mod.GetResources(ResourceType.Font))
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
                var fontExt = fontExts.FirstOrDefault(e => Core.Mod.ModFiles.DataFileExists(fileNameStart + e.ext));
                if (fontExt == null)
                    throw new Exception("Cannot find font file 'fonts/{0}' (tried with extensions {1})"
                        .F(fontId, fontExts.Select(f => f.ext).JoinWith(", ")));

                _fonts.Add(fontId, new FontFile(fileNameStart + fontExt.ext, fontExt.bitmap));
            }

            _fontsLoaded = true;
        }

        public bool Exists(string fontId)
        {
            if (!_fontsLoaded)
                LoadFonts();

            return _fonts.ContainsKey(fontId);
        }

        public bool IsBitmapFont(string fontId)
        {
            if (!_fontsLoaded)
                LoadFonts();

            Debug.CheckAssertion(Exists(fontId), "No such font: '{0}'".F(fontId));
            return (_fonts[fontId].BitmapFont != null);
        }

        public Surface GetBitmapFont(string fontId)
        {
            if (!_fontsLoaded)
                LoadFonts();

            Debug.CheckAssertion(Exists(fontId), "No such font: '{0}'".F(fontId));
            Debug.CheckAssertion(IsBitmapFont(fontId), "This is not a bitmap font: '{0}'".F(fontId));
            return (_fonts[fontId].BitmapFont);
        }

        public IntPtr GetOutlineFont(string fontId, int size)
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
