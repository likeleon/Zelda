using System;
using Zelda.Game.Lowlevel;
using Zelda.Game.Entities;
using Zelda.Game.Movements;

namespace Zelda.Game.Heroes
{
    class HurtState : State
    {
        readonly bool _hasSource;
        readonly Point _sourceXY;
        readonly int _damage;
        uint _endHurtDate;

        public HurtState(Hero hero, Point sourceXY, int damage)
            : base(hero, "hurt")
        {
            _hasSource = (_sourceXY != null);
            _sourceXY = _hasSource ? sourceXY : new Point(0, 0);
            _damage = damage;
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            Sound.Play("hero_hurt");

            uint invincibilityDuration = 2000;
            Hero.SetInvincible(true, invincibilityDuration);
            Sprites.SetAnimationHurt();
            Sprites.Blink(invincibilityDuration);

            if (_hasSource)
            {
                double angle = Geometry.GetAngle(_sourceXY, Hero.XY);
                StraightMovement movement = new StraightMovement(false, true);
                movement.MaxDistance = 24;
                movement.SetSpeed(120);
                movement.SetAngle(angle);
                Hero.SetMovement(movement);
            }
            _endHurtDate = Framework.Now + 200;

            if (_damage != 0)
            {
                int lifePoints = Math.Max(1, _damage / (Equipment.GetAbility(Ability.Tunic)));

                Equipment.RemoveLife(lifePoints);
                if (Equipment.HasAbility(Ability.Tunic))
                    Equipment.NotifyAbilityUsed(Ability.Tunic);
            }
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            Hero.ClearMovement();
        }

        public override void Update()
        {
            base.Update();

            if ((Hero.Movement != null && Hero.Movement.IsFinished) ||
                Framework.Now >= _endHurtDate)
            {
                Hero.ClearMovement();
                Hero.StartStateFromGround();
            }
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended)
            {
                uint diff = Framework.Now - WhenSuspended;
                _endHurtDate += diff;
            }
        }

        public override bool CanBeHurt(MapEntity attacker)
        {
            return false;
        }
    }
}
