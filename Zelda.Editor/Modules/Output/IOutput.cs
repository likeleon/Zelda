using System.IO;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.Output
{
    public interface IOutput : ITool
    {
        TextWriter Writer { get; }
        void AppendLine(string text);
        void Append(string text);
        void Clear();
    }
}
