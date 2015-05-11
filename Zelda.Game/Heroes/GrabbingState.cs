using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class GrabbingState : State
    {
        public GrabbingState(Hero hero)
            : base(hero, "grabbing")
        {
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);
            Sprites.SetAnimationGrabbing();
        }

        public override void Update()
        {
            if (IsSuspended)
                return;

            Direction8 wantedDirection8 = Commands.GetWantedDirection8();
            Direction8 spriteDirection8 = Sprites.AnimationDirection8;

            if (Commands.IsCommandPressed(GameCommand.Action))
                Hero.SetState(new FreeState(Hero));
            //else if (wantedDirection8 == spriteDirection8)
            //    Hero.SetState(new PushingState(Hero));
            //else if (wantedDirection8 == (Direction8)(((int)spriteDirection8 + 4) % 8))
            //    Hero.SetState(new PullingState(Hero));
        }

        public override bool IsGrabbingOrPulling
        {
            get { return true; }
        }
    }
}
