using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptBlock : ScriptEntity
    {
        readonly Block _block;
        internal Block Block
        {
            get { return _block; }
        }

        internal ScriptBlock(Block block)
            : base(block)
        {
            _block = block;
        }
    }
}
