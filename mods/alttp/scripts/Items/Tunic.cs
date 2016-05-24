using Zelda.Game;

namespace Alttp.Items
{
    [Id("tunic")]
    class Tunic : EquipmentItem
    {
        [ObjectCreator.UseCtor]
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
