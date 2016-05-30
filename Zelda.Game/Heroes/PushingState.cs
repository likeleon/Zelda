using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Entities;
using Zelda.Game.Movements;

namespace Zelda.Game.Heroes
{
    class PushingState : State
    {
        public PushingState(Hero hero)
            : base(hero, "pushing")
        {
            _pushingDirection = Direction4.Right;
        }

        Direction4 _pushingDirection;
        Detector _pushedEntity;
        PathMovement _pushingMovement;

        public override void Start(State previousState)
        {
            base.Start(previousState);
            _pushingDirection = Sprites.AnimationDirection;
            Sprites.SetAnimationPushing();
        }

        public override void Stop(State nextState)
        {
            base.Stop(nextState);

            if (IsMovingGrabbedEntity)
            {
                Hero.ClearMovement();
                _pushedEntity.Update();
                StopMovingPushedEntity();
            }
        }

        public override void Update()
        {
            base.Update();

            if (IsMovingGrabbedEntity)
                return;

            // 장애물이 없다면 미는 것을 멈춥니다
            if (!Hero.IsFacingObstacle())
                Hero.SetState(new FreeState(Hero));

            // 플레이어가 방향을 바꾸면 미는 것을 멈춥니다
            else if (Commands.GetWantedDirection8() != _pushingDirection.ToDirection8())
            {
                if (Commands.IsCommandPressed(GameCommand.Action))
                    Hero.SetState(new GrabbingState(Hero));
                else
                    Hero.SetState(new FreeState(Hero));
            }

            else
            {
                Detector facingEntity = Hero.FacingEntity;
                if (facingEntity != null)
                {
                    if (facingEntity.Type == EntityType.Block)
                        Hero.TrySnapToFacingEntity();

                    if (facingEntity.StartMovementByHero())
                    {
                        List<Direction8> path = Enumerable.Repeat(_pushingDirection.ToDirection8(), 2).ToList();
                        _pushingMovement = new PathMovement(path, 40, false, false, false);
                        Hero.SetMovement(_pushingMovement);
                        _pushedEntity = facingEntity;
                        _pushedEntity.NotifyMovingBy(Hero);
                    }
                }
            }
        }

        public override bool IsMovingGrabbedEntity
        {
            get { return _pushedEntity != null; }
        }

        public override void NotifyGrabbedEntityCollision()
        {
            StopMovingPushedEntity();
        }

        public override void NotifyMovementFinished()
        {
            if (IsMovingGrabbedEntity)
            {
                _pushedEntity.Update();
                StopMovingPushedEntity();
            }
        }

        public override void NotifyPositionChanged()
        {
            if (IsMovingGrabbedEntity)
            {
                // 엔티티가 8 픽셀 이상 이동했고 그리드에 정렬되었다면 이제 이동을 멈춥니다
                bool horizontal = (int)_pushingDirection % 2 == 0;
                bool hasReachedGrid =
                    _pushingMovement.TotalDistanceCovered > 8 &&
                    ((horizontal && _pushedEntity.IsAlignedToGridX) || (!horizontal && _pushedEntity.IsAlignedToGridY));
                if (hasReachedGrid)
                    StopMovingPushedEntity();
            }
        }

        void StopMovingPushedEntity()
        {
            if (_pushedEntity != null)
            {
                _pushedEntity.StopMovementByHero();

                // 주인공이 블럭보다 먼저 움직이기 때문에 몇 픽셀 더 이동했을 수 있습니다.

                switch (_pushingDirection)
                {
                    case Direction4.Right:
                        Hero.X = _pushedEntity.X - 16;
                        break;

                    case Direction4.Up:
                        Hero.Y = _pushedEntity.Y + 16;
                        break;

                    case Direction4.Left:
                        Hero.X = _pushedEntity.X + 16;
                        break;

                    case Direction4.Down:
                        Hero.Y = _pushedEntity.Y - 16;
                        break;
                }

                Hero.ClearMovement();
                _pushingMovement = null;
                Entity entityJustMoved = _pushedEntity;
                _pushedEntity = null;
                entityJustMoved.NotifyMovedBy(Hero);
            }

            if (!IsCurrentState)
            {
                // 다른 상태가 이미 시작된 경우(예를 들면, TreasureState), 이를 덮어쓰지 않습니다
                return;
            }

            if (!Commands.IsCommandPressed(GameCommand.Action))
            {
                if (Commands.GetWantedDirection8() != _pushingDirection.ToDirection8())
                    Hero.SetState(new FreeState(Hero));
            }
            else
            {
                Hero.SetState(new GrabbingState(Hero));
            }
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

        public override bool CanBeHurt(Entity attacker)
        {
            return !IsMovingGrabbedEntity;
        }
    }
}
