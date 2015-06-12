using Zelda.Game.Script;

namespace Alttp.Items
{
    [Id("rupee_bag")]
    class RupeeBag : ScriptItem
    {
        protected override void OnCreated()
        {
            SavegameVariable = "i1034";
        }

        //TODO: OnVariantChanged
    }
}
