using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptDynamicTile : ScriptEntity
    {
        internal DynamicTile DynamicTile { get; private set; }

        internal ScriptDynamicTile(DynamicTile dynamicTile)
            : base(dynamicTile)
        {
            DynamicTile = dynamicTile;
        }
    }
}
