using System.Threading.Tasks;

namespace Zelda.Editor.Core
{
    interface IPersistedDocument : IDocument
    {
        string FileName { get; }
        string FilePath { get; }

        Task Load(string filePath);
    }
}
