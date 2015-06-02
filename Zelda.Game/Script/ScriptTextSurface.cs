using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptTextSurface : ScriptDrawable
    {
        readonly TextSurface _textSurface;

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
                    if (!StringResource.Exists(textKey))
                        throw new ArgumentException("No value with key '{0}' in strings.xml for language '{1}'"
                            .F(textKey, Language.LanguageCode));
                    textSurface.SetText(StringResource.GetString(textKey));
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
    }
}
