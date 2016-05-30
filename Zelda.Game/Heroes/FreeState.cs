using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class FreeState : PlayerMovementState
    {
        public FreeState(Hero hero)
            : base(hero, "free")
        {
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
        }

        public override void SetAnimationStopped()
        {
            Sprites.SetAnimationStoppedNormal();
        }

        public override void SetAnimationWalking()
        {
            Sprites.SetAnimationWalkingNormal();
        }

        public override void NotifyActionCommandPressed()
        {
            Detector facingEntity = Hero.FacingEntity;
            if (facingEntity != null)
            {
                if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None ||
                    CommandsEffects.IsActionCommandActingOnFacingEntity)
                    facingEntity.NotifyActionCommandPressed();
            }
        }

        public override CarriedObject.Behavior PreviousCarriedItemBehavior
        {
            get { return CarriedObject.Behavior.Destroy; }
        }

        public override bool IsFree
        {   
            get { return true; }
        }

        public override bool CanStartItem(EquipmentItem item)
        {
            return Hero.GroundBelow != Ground.Hole;
        }
    }
}
