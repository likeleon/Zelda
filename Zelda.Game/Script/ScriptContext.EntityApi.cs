using System;
using RawGame = Zelda.Game.Game;
using RawHero = Zelda.Game.Entities.Hero;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        internal static void NotifyHeroBrandishTreasure(Treasure treasure)
        {
            string dialogId = "_treasure.{0}.{1}".F(treasure.ItemName, treasure.Variant);

            Action dialogCallback = () =>
            {
                TreasureDialogFinished(treasure.Item, treasure.Variant, treasure.SavegameVariable);
            };

            if (!DialogResource.Exists(dialogId))
            {
                Debug.Error("Missing treasure dialog: '{0}'".F(dialogId));
                dialogCallback();
            }
            else
                treasure.Game.StartDialog(dialogId, dialogCallback);
        }

        static void TreasureDialogFinished(EquipmentItem item, int treasureVariant, string treasureSavegameVariable)
        {
            Debug.CheckAssertion(item.Game != null, "Equipment item without game");

            RawGame game = item.Game;
            RawHero hero = game.Hero;

            if (hero.IsBrandishingTreasure)
            {
                // 스크립트에서 주인공의 상태를 바꾸지 않았다면, 여기서 Treasure 상태를 풀어줍니다
                hero.StartFree();
            }
        }
    }
}
