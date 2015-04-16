
namespace Zelda.Game
{
    // 엔진에서 처리할 수 있는 내장된 커맨드들로 키보드나 조이패드와 매핑됩니다
    enum GameCommand
    {
        None = -1,
        Action,
        Attack,
        Item1,
        Item2,
        Pause,
        Right,
        Up,
        Left,
        Down
    }
}
