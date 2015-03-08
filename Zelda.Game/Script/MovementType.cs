using System.ComponentModel;

namespace Zelda.Game.Script
{
    public enum MovementType
    {
        [Description("직선 궤적을 따라 이동합니다")]
        Straight,
        
        [Description("Straight와 비슷하지만 지정한 점이나 이동 객체로의 방향으로 이동합니다")]
        Target
    }
}
