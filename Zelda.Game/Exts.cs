using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zelda.Game
{
    public static class Exts
    {
        public static string F(this string fmt, params object[] args)
        {
            return string.Format(fmt, args);
        }

        public static IEnumerable<string> GetNamespaces(this Assembly a)
        {
            return a.GetTypes().Select(t => t.Name).Distinct().Where(n => n != null);
        }

        public static bool HasAttribute<T>(this MemberInfo mi)
        {
            return (mi.GetCustomAttributes(typeof(T), true).Length != 0);
        }
    }
}
