using System;
using RawGame = Zelda.Game.Game;
using RawHero = Zelda.Game.Entities.Hero;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
        internal static void NotifyHeroBrandishTreasure(Treasure treasure, Action callback)
        {
            string dialogId = "_treasure.{0}.{1}".F(treasure.ItemName, treasure.Variant);

            Action<object> dialogCallback = (_) => TreasureDialogFinished(treasure.Item, treasure.Variant, treasure.SavegameVariable, callback);

            if (!CurrentMod.DialogExists(dialogId))
            {
                Debug.Error("Missing treasure dialog: '{0}'".F(dialogId));
                dialogCallback(null);
            }
            else
                treasure.Game.StartDialog(dialogId, null, dialogCallback);
        }

        static void TreasureDialogFinished(EquipmentItem item, int treasureVariant, string treasureSavegameVariable, Action callback)
        {
            Debug.CheckAssertion(item.Game != null, "Equipment item without game");

            var game = item.Game;
            var hero = game.Hero;
            var treasure = new Treasure(game, item.Name, treasureVariant, treasureSavegameVariable);

            if (callback != null)
                CoreToScript.Call(callback);

            item.ScriptItem.NotifyObtained(treasure);

            if (hero.IsBrandishingTreasure)
            {
                // 스크립트에서 주인공의 상태를 바꾸지 않았다면, 여기서 Treasure 상태를 풀어줍니다
                hero.StartFree();
            }
        }
    }
}
