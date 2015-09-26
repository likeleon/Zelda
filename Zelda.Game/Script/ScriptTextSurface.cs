﻿using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptTextSurface : ScriptDrawable
    {
        readonly TextSurface _textSurface;

        public string Text { get { return _textSurface.Text; } }
        public Size Size { get { return _textSurface.Size; } }

        public static ScriptTextSurface Create(
            string font = null,
            TextRenderingMode renderingMode = TextRenderingMode.Solid,
            TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left, 
            TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Middle,
            Color? color = null,
            int? fontSize = null,
            string text = null,
            string textKey = null)
        {
            return ScriptToCore.Call(() =>
            {
                font = font ?? FontResource.GetDefaultFontId();
                if (!FontResource.Exists(font))
                    throw new ArgumentException("No such font: '{0}'".F(font), "font");

                TextSurface textSurface = new TextSurface(0, 0);
                textSurface.SetFont(font);
                textSurface.SetRenderingMode(renderingMode);
                textSurface.SetHorizontalAlignment(horizontalAlignment);
                textSurface.SetVerticalAlignment(verticalAlignment);
                textSurface.SetTextColor(color ?? Color.White);
                textSurface.SetFontSize(fontSize ?? TextSurface.DefaultFontSize);

                if (!String.IsNullOrEmpty(text))
                    textSurface.SetText(text);
                else if (!String.IsNullOrEmpty(textKey))
                {
                    if (!CurrentMod.StringExists(textKey))
                        throw new ArgumentException("No value with key '{0}' in strings.xml for language '{1}'".F(textKey, CurrentMod.Language));

                    textSurface.SetText(CurrentMod.GetString(textKey));
                }
            
                AddDrawable(textSurface);
                return new ScriptTextSurface(textSurface);
            });
        }

        ScriptTextSurface(TextSurface textSurface)
            : base(textSurface)
        {
            _textSurface = textSurface;
        }

        public void SetColor(Color color)
        {
            ScriptToCore.Call(() => _textSurface.SetTextColor(color));
        }

        public void SetText(string text)
        {
            ScriptToCore.Call(() => _textSurface.SetText(text));
        }

        public void SetTextKey(string key)
        {
            ScriptToCore.Call(() =>
            {
                if (!CurrentMod.StringExists(key))
                    throw new ArgumentException("No value with key '{0}' in strings.xml for language '{1}'".F(key, CurrentMod.Language));
                
                _textSurface.SetText(CurrentMod.GetString(key));
            });
        }
        
        public void SetFont(string fontId)
        {
            ScriptToCore.Call(() =>
            {
                if (!FontResource.Exists(fontId))
                    throw new ArgumentException("No such font: '{0}'".F(fontId));
                _textSurface.SetFont(fontId);
            });
        }

        public void SetFontSize(int fontSize)
        {
            ScriptToCore.Call(() => _textSurface.SetFontSize(fontSize));
        }
    }
}
