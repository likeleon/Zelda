using System;

namespace Zelda.Editor.Modules.Progress
{
    public interface IJob
    {
        // 유저에게 노출할 이름
        string Name { get; }

        // 총 작업 유닛
        int TotalWork { get; }

        // 시스템 잡 유무
        // 시스템 잡으로 설정되면 기본적으로 어떤 UI도 노출되지 않는다. 이외 동작은 다른 작업과 동일.
        bool IsSystem { get; }

        // UI 엔드 유저에 의해 시작되었는지?
        // 이 값을 true로 설정하면 작업 시작시 진행 다이얼로그가 유저에게 표시된다
        bool IsUser { get; }

        // 표시할 아이콘
        Uri IconSource { get; }

        // 실행할 액션
        Action<IProgressMonitor> Action { get; }
    }
}
