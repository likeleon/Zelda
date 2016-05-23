using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp.Items
{
    [Id("tunic")]
    class Tunic : EquipmentItem
    {
        public Tunic(Equipment equipment, string name)
        : base(equipment, name)
        {
            SavegameVariable = "i1128";
        }

        public override void OnObtained(int variant, string savegameVariable)
        {
            Savegame.SetAbility(Ability.Tunic, variant);
        }
    }
}
