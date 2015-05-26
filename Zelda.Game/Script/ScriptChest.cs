using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptChest : ScriptEntity
    {
        readonly Chest _chest;
        internal Chest Chest
        {
            get { return _chest; }
        }

        internal ScriptChest(Chest chest)
            : base(chest)
        {
            _chest = chest;
        }
    }
}
