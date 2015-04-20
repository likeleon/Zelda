using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public virtual int WantedDirection8
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
                    if (WantedDirection8 != -1)
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
    }
}
