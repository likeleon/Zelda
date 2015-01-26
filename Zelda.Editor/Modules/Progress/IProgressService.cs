using System.Threading.Tasks;
using Zelda.Editor.Core;

namespace Zelda.Editor.Modules.Progress
{
    public interface IProgressService
    {
        Task Run(IJob job);
    }
}
