using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Entities;
using Zelda.Game.Movements;

namespace Zelda.Game.Heroes
{
    class PullingState : State
    {
        public PullingState(Hero hero)
            : base(hero, "pulling")
        {
        }

        Detector _pulledEntity;
        PathMovement _pullingMovement;

        public override void Start(State previousState)
        {
            base.Start(previousState);

            _pulledEntity = null;
            Sprites.SetAnimationPulling();
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            if (IsMovingGrabbedEntity)
            {
                Hero.ClearMovement();
                _pulledEntity.Update();
                StopMovingPulledEntity();
            }
        }

        public override void Update()
        {
            base.Update();

            if (IsMovingGrabbedEntity)
                return;

            Direction8 wantedDirection = Commands.GetWantedDirection8();
            Direction8 oppositeDirection = Sprites.AnimationDirection8.GetOpposite();

            // 액션키를 더 이상 누르지 않고 있거나 장애물이 없다면 당기는 것을 멈춥니다
            if (!Commands.IsCommandPressed(GameCommand.Action) ||
                !Hero.IsFacingObstacle())
            {
                Hero.SetState(new FreeState(Hero));
            }

            // 플레이어가 방향을 바꾸면 당기는 것을 멈춥니다
            else if (wantedDirection != oppositeDirection)
            {
                Hero.SetState(new GrabbingState(Hero));
            }

            // 장애물이 주인공이 당길 수 있는 것인지 확인합니다
            else
            {
                Detector facingEntity = Hero.FacingEntity;
                if (facingEntity != null)
                {
                    if (facingEntity.Type == EntityType.Block)
                        Hero.TrySnapToFacingEntity();

                    if (facingEntity.StartMovementByHero())
                    {
                        List<Direction8> path = Enumerable.Repeat(oppositeDirection, 2).ToList();
                        _pullingMovement = new PathMovement(path, 40, false, false, false);
                        Hero.SetMovement(_pullingMovement);
                        _pulledEntity = facingEntity;
                        _pulledEntity.NotifyMovingBy(Hero);
                    }
                }
            }
        }

        public override bool IsGrabbingOrPulling
        {
            get { return true; }
        }

        public override bool IsMovingGrabbedEntity
        {
            get { return _pulledEntity != null; }
        }

        public override void NotifyGrabbedEntityCollision()
        {
            StopMovingPulledEntity();
        }

        public override void NotifyMovementFinished()
        {
            if (IsMovingGrabbedEntity)
            {
                _pulledEntity.Update();
                StopMovingPulledEntity();
            }
        }


        public override void NotifyPositionChanged()
        {
            if (IsMovingGrabbedEntity)
            {
                bool horizontal = Sprites.AnimationDirection.IsHorizontal();
                bool hasReachedGrid = 
                    _pullingMovement.TotalDistanceCovered > 8 &&
                    ((horizontal && _pulledEntity.IsAlignedToGridX) || (!horizontal && _pulledEntity.IsAlignedToGridY));

                if (hasReachedGrid)
                    StopMovingPulledEntity();
            }
        }

        void StopMovingPulledEntity()
        {
            if (_pulledEntity != null)
            {
                _pulledEntity.StopMovementByHero();

                Direction4 direction = Sprites.AnimationDirection;
                switch (direction)
                {
                    case Direction4.Right:
                        Hero.X = _pulledEntity.X - 16;
                        break;

                    case Direction4.Up:
                        Hero.Y = _pulledEntity.Y + 16;
                        break;

                    case Direction4.Left:
                        Hero.X = _pulledEntity.X + 16;
                        break;

                    case Direction4.Down:
                        Hero.Y = _pulledEntity.Y - 16;
                        break;
                }

                Hero.ClearMovement();
                _pullingMovement = null;
                MapEntity entityJustMoved = _pulledEntity;
                _pulledEntity = null;
                entityJustMoved.NotifyMovedBy(Hero);
            }

            Hero.SetState(new GrabbingState(Hero));
        }

        public override bool IsShallowWaterObstacle
        {
            get { return true; }
        }

        public override bool IsDeepWaterObstacle
        {
            get { return true; }
        }

        public override bool IsHoleObstacle
        {
            get { return true; }
        }

        public override bool IsLavaObstacle
        {
            get { return true; }
        }

        public override bool IsPrickleObstacle
        {
            get { return true; }
        }

        public override bool CanBeHurt(MapEntity attacker)
        {
            return !IsMovingGrabbedEntity;
        }
    }
}
