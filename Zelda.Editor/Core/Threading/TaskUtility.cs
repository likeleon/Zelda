using System.Threading.Tasks;

namespace Zelda.Editor.Core.Threading
{
    public class TaskUtility
    {
        public static readonly Task Completed = Task.FromResult(true);
    }
}
