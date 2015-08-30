using System;

namespace Zelda.Editor
{
    static class Exts
    {
        public static Uri ToIconUri(this string path)
        {
            return new Uri(path, UriKind.Relative);
        }
    }
}
