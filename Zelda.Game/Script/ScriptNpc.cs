using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptNpc : ScriptEntity
    {
        readonly Npc _npc;
        internal Npc Npc
        {
            get { return _npc; }
        }

        internal ScriptNpc(Npc npc)
            : base(npc)
        {
            _npc = npc;
        }
    }
}
