using System.Collections.Generic;

namespace Zelda.Editor.Core.Services
{
    interface IEditorProvider
    {
        bool Handles(string path);

        IDocument Create();
        IDocument Open(string path);
        IDocument Find(IEnumerable<IDocument> documents, string path);
    }
}
