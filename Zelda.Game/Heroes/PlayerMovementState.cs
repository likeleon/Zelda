using Zelda.Game.Entities;
using Zelda.Game.Movements;

namespace Zelda.Game.Heroes
{
    // 모든 "플레이어에 의해 주인공이 움직이는 상태"를 위한 가상 클래스입니다
    abstract class PlayerMovementState : State
    {
        PlayerMovement _playerMovement;

        protected PlayerMovement PlayerMovement
        {
            get { return _playerMovement; }
        }

        public override Direction8 WantedMovementDirection8
        {
            get { return _playerMovement.WantedDirection8; }
        }

        public PlayerMovementState(Hero hero, string stateName)
            : base(hero, stateName)
        {
        }

        public override void Start(State previousState)
        {
            base.Start(previousState);

            _playerMovement = new PlayerMovement(Hero.WalkingSpeed);
            Hero.SetMovement(_playerMovement);

            if (IsCurrentState)
            {
                _playerMovement.ComputeMovement();
                if (IsCurrentState)
                {
                    if (WantedMovementDirection8 != Direction8.None)
                        SetAnimationWalking();
                    else
                        SetAnimationStopped();
                }
            }
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            Hero.ClearMovement();
            Sprites.SetAnimationStoppedNormal();
            _playerMovement = null;
        }

        public override void SetMap(Map map)
        {
            base.SetMap(map);
            SetAnimationStopped();
        }

        public virtual void SetAnimationStopped()
        {
        }

        public virtual void SetAnimationWalking()
        {
        }

        public override void NotifyMovementChanged()
        {
            // 이동에 변화가 있습니다. 스프라이트의 애니메이션을 갱신시킵니다.
            bool movementWalking = (WantedMovementDirection8 != Direction8.None);
            bool spritesWalking = Sprites.IsWalking;

            if (movementWalking && !spritesWalking)
                SetAnimationWalking();
            else if (!movementWalking && spritesWalking)
                SetAnimationStopped();
        }

        public override void NotifyLayerChanged()
        {
            Hero.UpdateMovement();
        }

        public override bool CanBeHurt(MapEntity attacker)
        {
            return true;
        }
    }
}
