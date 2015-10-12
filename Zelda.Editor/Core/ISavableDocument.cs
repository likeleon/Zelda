using System.Threading.Tasks;

namespace Zelda.Editor.Core
{
    interface ISavableDocument
    {
        Task Save();
    }
}
