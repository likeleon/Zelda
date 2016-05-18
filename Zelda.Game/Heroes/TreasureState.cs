using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Entities;
using Zelda.Game.Script;

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
                Audio.Play(soundId);

            _treasure.GiveToPlayer();

            ScriptContext.NotifyHeroBrandishTreasure(_treasure, _callback);
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

        public override CarriedItem.Behavior PreviousCarriedItemBehavior
        {
            get { return CarriedItem.Behavior.Destroy; }
        }

        public override bool IsBrandishingTreasure
        {
            get { return true; }
        }
    }
}
