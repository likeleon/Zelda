using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class LiftingState : State
    {
        readonly CarriedItem _liftedItem;

        public LiftingState(Hero hero, CarriedItem liftedItem)
            : base(hero, "lifting")
        {
            _liftedItem = liftedItem;
            Debug.CheckAssertion(_liftedItem != null, "Missing lifted item");
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            _liftedItem.SetMap(Map);

            CommandsEffects.ActionCommandEffect = ActionCommandEffect.Throw;
            Hero.FacingEntity = null;
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            if (_liftedItem != null)
            {
                // TODO: CarriedItem 처리

                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
        }

        public override void Update()
        {
            base.Update();

            _liftedItem.Update();
        }
    }
}
