using System.Threading.Tasks;

namespace Zelda.Editor.Core.Services
{
    interface IEditorProvider
    {
        bool Handled(string path);

        IDocument Create();

        Task Open(IDocument document, string path);
    }
}
