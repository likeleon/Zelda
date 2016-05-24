using Zelda.Game;

namespace Alttp.Items
{
    [Id("rupee_bag")]
    class RupeeBag : EquipmentItem
    {
        [ObjectCreator.UseCtor]
        public RupeeBag(Equipment equipment, string name)
            : base(equipment, name)
        {
            SavegameVariable = "i1034";
        }

        //TODO: OnVariantChanged
    }
}
