using System;
using Zelda.Game.Entities;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Heroes
{
    class TreasureState : State
    {
        readonly Treasure _treasure;
        readonly Action _callback;

        public TreasureState(Hero hero, Treasure treasure, Action callback)
            : base(hero, "treasure")
        {
            _treasure = treasure;
            _treasure.CheckObtainable();
            _callback = callback;
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            Sprites.SaveAnimationDirection();
            Sprites.SetAnimationBrandish();

            string soundId = _treasure.Item.SoundWhenBrandished;
            if (!String.IsNullOrEmpty(soundId))
                Core.Audio?.Play(soundId);

            _treasure.GiveToPlayer();

            NotifyHeroBrandishTreasure(_treasure, _callback);
        }

        void NotifyHeroBrandishTreasure(Treasure treasure, Action callback)
        {
            string dialogId = "_treasure.{0}.{1}".F(treasure.ItemName, treasure.Variant);

            if (!Core.Mod.DialogExists(dialogId))
                throw new Exception("Missing treasure dialog: '{0}'".F(dialogId));

            treasure.Game.StartDialog(dialogId, null, _ => TreasureDialogFinished(treasure.Item, treasure.Variant, treasure.SavegameVariable, callback));
        }

        void TreasureDialogFinished(EquipmentItem item, int treasureVariant, string treasureSavegameVariable, Action callback)
        {
            if (item.Game == null)
                throw new InvalidOperationException("Equipment item without game");


            callback?.Invoke();

            var treasure = new Treasure(item.Game, item.Name, treasureVariant, treasureSavegameVariable);
            item.OnObtained(treasure.Variant, treasure.IsSaved ? treasure.SavegameVariable : null);

            if (item.Game.Hero.IsBrandishingTreasure)
                item.Game.Hero.StartFree(); // 스크립트에서 주인공의 상태를 바꾸지 않았다면, 여기서 Treasure 상태를 풀어줍니다
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            Sprites.RestoreAnimationDirection();
        }

        public override void DrawOnMap()
        {
            base.DrawOnMap();

            int x = Hero.X;
            int y = Hero.Y;

            Rectangle cameraPosition = Map.CameraPosition;
            _treasure.Draw(Map.VisibleSurface,
                x - cameraPosition.X,
                y - 24 - cameraPosition.Y);
        }

        public override CarriedObject.Behavior PreviousCarriedItemBehavior
        {
            get { return CarriedObject.Behavior.Destroy; }
        }

        public override bool IsBrandishingTreasure
        {
            get { return true; }
        }
    }
}
