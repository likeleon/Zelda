using System;

namespace Zelda.Game.Entities
{
    [Flags]
    enum CollisionMode
    {
        None            = 0x0000,
        Overlapping     = 0x0001,
        Containing      = 0x0002,
        Origin          = 0x0004,
        Facing          = 0x0008,
        Touching        = 0x0010,
        Center          = 0x0020,
        Sprite          = 0x0040,
        Custom          = 0x0080
    }
}
