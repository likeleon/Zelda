
namespace Zelda.Editor.Modules.Progress
{
    public interface IProgressMonitor
    {
        // 취소가 요청된 상태인지를 리턴
        bool IsCanceled { get; } 

        // 취소가 요청된 상태이면 OperationCanceledException을 발생
        void ThrowIfCancellationRequested();

        // 지정한 양만큼의 작업이 완료되었음을 알림
        void Worked(string message, int work);
    }
}
