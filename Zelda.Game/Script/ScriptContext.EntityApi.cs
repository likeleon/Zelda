using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Entities;

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

            Zelda.Game.Game game = item.Game;
            Hero hero = game.Hero;

            if (hero.IsBrandishingTreasure)
            {
                // 스크립트에서 주인공의 상태를 바꾸지 않았다면, 여기서 Treasure 상태를 풀어줍니다
                hero.StartFree();
            }
        }
    }
}
