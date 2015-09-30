using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zelda.Editor.Core.Services
{
    interface IEditorProvider
    {
        bool Handles(string path);

        IDocument Create();
        IDocument Find(IEnumerable<IDocument> documents, string path);

        Task Open(IDocument document, string path);
    }
}
