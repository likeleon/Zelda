using System;
using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class HeroSprites
    {
        public string TunicSpriteId { get; private set; }
        public Direction4 AnimationDirection => _tunicSprite.CurrentDirection;
        public Direction8 AnimationDirection8 => AnimationDirection.ToDirection8();
        public bool IsBlinking { get; private set; }
        public bool IsWalking { get; private set; }
        public CarriedObject LiftedItem { get; set; }

        static readonly Direction4[,] _animationDirections = new Direction4[,]
        {
            { Direction4.Right, Direction4.None },   // right
            { Direction4.Right, Direction4.Up  },   // right-up: right or up
            { Direction4.Up,   Direction4.None },   // up
            { Direction4.Left,  Direction4.Up  },   // left-up: left or up
            { Direction4.Left,  Direction4.None },   // left
            { Direction4.Left,  Direction4.Down },   // left-down: left or down
            { Direction4.Down,  Direction4.None },   // down
            { Direction4.Right, Direction4.Down }    // right-down: right or down
        };

        readonly Hero _hero;
        readonly Equipment _equipment;
        bool _hasDefaultTunicSprite = true;
        Sprite _tunicSprite;
        Direction4 _animationDirectionSaved = Direction4.None;
        int _whenSuspended;
        int _endBlinkDate;

        public HeroSprites(Hero hero, Equipment equipment)
        {
            _hero = hero;
            _equipment = equipment;

            RebuildEquipment();
        }

        string GetDefaultTunicSpriteId()
        {
            int tunicLevel = _equipment.GetAbility(Ability.Tunic);
            return "hero/tunic" + tunicLevel;
        }

        public void SetTunicSpriteId(string spriteId)
        {
            if (spriteId == TunicSpriteId)
                return;

            TunicSpriteId = spriteId;

            string animation = null;
            Direction4 direction = Direction4.None;
            if (_tunicSprite != null)
            {
                animation = _tunicSprite.CurrentAnimation;
                direction = _tunicSprite.CurrentDirection;
                _hero.RemoveSprite(_tunicSprite);
                _tunicSprite = null;
            }

            _tunicSprite = Sprite.Create(spriteId, false);
            _tunicSprite.EnablePixelCollisions();
            if (animation != null)
            {
                _tunicSprite.SetCurrentAnimation(animation);
                _tunicSprite.SetCurrentDirection(direction);
            }

            _hasDefaultTunicSprite = spriteId == GetDefaultTunicSpriteId();
        }

        // 영웅과 장비품의 스프라이트와 사운드 데이터를 읽어들입니다.
        // 게임 시작 시점과 영웅 장비가 교체된 직후에 호출되어야 합니다.
        public void RebuildEquipment()
        {
            _hero.DefaultSpriteName = "tunic";

            // 방향 저장
            var animationDirection = _tunicSprite?.CurrentDirection;

            // 영웅 몸체
            if (_hasDefaultTunicSprite)
                SetTunicSpriteId(GetDefaultTunicSpriteId());

            // 방향 복구
            if (animationDirection.HasValue)
                SetAnimationDirection(animationDirection.Value);
        }

        public void DrawOnMap()
        {
            var x = _hero.X;
            var y = _hero.Y;

            var map = _hero.Map;

            var displayedXY = _hero.GetDisplayedXY();
            x = displayedXY.X;
            y = displayedXY.Y;

            map.DrawSprite(_tunicSprite, x, y);

            LiftedItem?.DrawOnMap();
        }

        public void SetSuspended(bool suspended)
        {
            _tunicSprite.SetSuspended(suspended);

            // 타이머
            int now = Core.Now;
            if (suspended)
                _whenSuspended = now;
            else if (_endBlinkDate != 0)
                _endBlinkDate += now - _whenSuspended;
        }

        public void Update()
        {
            // Keep the current sprites here in case they change from a script during the operation.
            var tunicSprite = _tunicSprite;

            tunicSprite.Update();

            _hero.CheckCollisionWithDetectors(tunicSprite);

            if (IsBlinking && _endBlinkDate != 0 && Core.Now >= _endBlinkDate)
                StopBlink();
        }

        public void NotifyMapStarted()
        {
            NotifyTilesetChanged();
        }

        public void NotifyTilesetChanged()
        {
            LiftedItem?.NotifyTilesetChanged();
        }

        public Direction4 GetAnimationDirection(Direction8 keysDirection, Direction8 realMovementDirection)
        {
            Direction4 result = Direction4.None;
            if (keysDirection == Direction8.None)
            {
                // 유효하지 않은 방향키 조합인 경우: 스프라이트 방향 변경 없음
                result = Direction4.None;
            }
            else if ((int)keysDirection % 2 == 0)
            {
                // 네 방향 중 한 방향만 입력한 경우: 해당 방향을 스프라이트에게 부여
                result = (Direction4)((int)keysDirection / 2);
            }
            // 이동이 대각선 방향인 경우: 두 방향 중 하나를 선택해야 합니다
            else if (_animationDirections[(int)realMovementDirection, 1] == AnimationDirection)
            {
                // 이미 스프라이트 방향이라면 두 번째 방향을 선택합니다
                result = _animationDirections[(int)realMovementDirection, 1];
            }
            else
            {
                // 그렇지 않다면 첫 번째 방향을 선택합니다
                result = _animationDirections[(int)realMovementDirection, 0];
            }
            return result;
        }

        public void SetAnimationDirection(Direction4 direction)
        {
            _tunicSprite.SetCurrentDirection(direction);
            LiftedItem?.GetSprite().RestartAnimation();
        }

        public void SaveAnimationDirection()
        {
            _animationDirectionSaved = AnimationDirection;
        }

        public void RestoreAnimationDirection()
        {
            SetAnimationDirection(_animationDirectionSaved);
        }

        public void SetAnimationStoppedCommon()
        {
            IsWalking = false;
        }

        public void SetAnimationStoppedNormal()
        {
            SetAnimationStoppedCommon();

            if (_equipment.HasAbility(Ability.Shield))
                throw new NotImplementedException();
            else
                SetTunicAnimation("stopped");
        }

        public void SetAnimationStoppedCarrying()
        {
            SetAnimationStoppedCommon();
            SetTunicAnimation("carrying_stopped");

            LiftedItem?.SetAnimationStopped();
        }

        public void SetAnimationWalkingCommon()
        {
            IsWalking = true;
        }

        public void SetAnimationWalkingNormal()
        {
            SetAnimationWalkingCommon();
            if (_equipment.HasAbility(Ability.Shield))
                throw new NotImplementedException();
            else
                SetTunicAnimation("walking");
        }

        public void SetAnimationWalkingCarrying()
        {
            SetAnimationWalkingCommon();
            SetTunicAnimation("carrying_walking");

            LiftedItem?.SetAnimationWalking();
        }

        public void SetAnimationGrabbing()
        {
            SetTunicAnimation("grabbing");
        }

        public void SetAnimationPulling()
        {
            SetTunicAnimation("pulling");
        }

        public void SetAnimationPushing()
        {
            SetTunicAnimation("pushing");
        }

        public void SetAnimationLifting()
        {
            SetTunicAnimation("lifting");
        }

        public void SetAnimationHurt()
        {
            SetTunicAnimation("hurt");
        }

        public void SetAnimationBrandish()
        {
            SetTunicAnimation("brandish");
            _tunicSprite.SetCurrentDirection(Direction4.Up);
        }

        void SetTunicAnimation(string animation)
        {
            _tunicSprite.SetCurrentAnimation(animation);
        }

        public void Blink(int duration)
        {
            int blinkDelay = 50;

            IsBlinking = true;
            _tunicSprite.SetBlinking(blinkDelay);

            if (duration == 0)
                _endBlinkDate = 0;  // 끝나지 않습니다
            else
                _endBlinkDate = Core.Now + duration;
        }

        public void StopBlink()
        {
            IsBlinking = false;
            _endBlinkDate = 0;

            _tunicSprite.SetBlinking(0);
        }
    }
}
