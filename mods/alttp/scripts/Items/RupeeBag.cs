using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp.Items
{
    [Id("rupee_bag")]
    class RupeeBag : EquipmentItem
    {
        public RupeeBag(Equipment equipment, string name)
            : base(equipment, name)
        {
            SavegameVariable = "i1034";
        }

        //TODO: OnVariantChanged
    }
}
