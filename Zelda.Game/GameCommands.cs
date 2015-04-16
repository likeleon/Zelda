
namespace Zelda.Game
{
    // 인게임 하이레벨 커맨드들과 그들의 키보드나 조이패드 매핑을 저장합니다.
    // 이 클래스는 키보드나 조이패드의 로우 레벨 이벤트를 받아 내장된 게임 커맨드들이 눌렸거나 릴리즈되었음을
    // 적당한 객체에게 알려주는 역할을 합니다.
    class GameCommands
    {
        readonly Game _game;

        public GameCommands(Game game)
        {
            _game = game;
        }
    }
}
