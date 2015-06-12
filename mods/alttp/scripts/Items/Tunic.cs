using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp.Items
{
    [Id("tunic")]
    class Tunic : ScriptItem
    {
        protected override void OnCreated()
        {
            SavegameVariable = "i1128";
        }

        protected override void OnObtained(int variant, string savegameVariable)
        {
            Game.SetAbility(Ability.Tunic, variant);
        }
    }
}
