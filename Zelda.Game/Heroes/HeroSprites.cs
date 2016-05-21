using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Entities;

namespace Zelda.Game.Heroes
{
    class HeroSprites
    {
        readonly Hero _hero;
        readonly Equipment _equipment;

        public HeroSprites(Hero hero, Equipment equipment)
        {
            _hero = hero;
            _equipment = equipment;

            RebuildEquipment();
        }

        #region Tunic
        public string TunicSpriteId { get; private set; }

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
                _tunicSprite = null;
            }

            _tunicSprite = new Sprite(spriteId);
            _tunicSprite.EnablePixelCollisions();
            if (!String.IsNullOrEmpty(animation))
            {
                _tunicSprite.SetCurrentAnimation(animation);
                _tunicSprite.SetCurrentDirection(direction);
            }
        }

        Sprite _tunicSprite;
        #endregion

        // 영웅과 장비품의 스프라이트와 사운드 데이터를 읽어들입니다.
        // 게임 시작 시점과 영웅 장비가 교체된 직후에 호출되어야 합니다.
        public void RebuildEquipment()
        {
            Direction4 animationDirection = Direction4.None;
            if (_tunicSprite != null)
            {
                // 방향 저장
                animationDirection = _tunicSprite.CurrentDirection;
            }

            // 영웅 몸체
            SetTunicSpriteId(GetDefaultTunicSpriteId());

            // 방향 복구
            if (animationDirection != Direction4.None)
                SetAnimationDirection(animationDirection);
        }

        public void DrawOnMap()
        {
            Map map = _hero.Map;

            Point xy = _hero.GetDisplayedXY();
            map.DrawSprite(_tunicSprite, xy.X, xy.Y);

            if (LiftedItem != null)
                LiftedItem.DrawOnMap();
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
            _tunicSprite.Update();

            _hero.CheckCollisionWithDetectors(_tunicSprite);

            if (LiftedItem != null && _walking)
                LiftedItem.Sprite.SetCurrentFrame(_tunicSprite.CurrentFrame % 3);

            if (IsBlinking &&
                _endBlinkDate != 0 &&
                Core.Now >= _endBlinkDate)
            {
                StopBlink();
            }
        }

        public void NotifyMapStarted()
        {
            NotifyTilesetChanged();
        }

        public void NotifyTilesetChanged()
        {
            if (LiftedItem != null)
                LiftedItem.NotifyTilesetChanged();
        }

        #region 애니메이션
        public Direction4 AnimationDirection
        {
            get { return _tunicSprite.CurrentDirection; }
        }
        
        public Direction8 AnimationDirection8
        {
            get { return AnimationDirection.ToDirection8(); }
        }

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
            Debug.CheckAssertion(direction >= 0 && (int)direction < 4, "Invalid direction for SetAnimationDirection");

            _tunicSprite.SetCurrentDirection(direction);
        }

        Direction4 _animationDirectionSaved = Direction4.None;

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
            _walking = false;
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

            if (LiftedItem != null)
                LiftedItem.SetAnimationStopped();
        }

        public void SetAnimationWalkingCommon()
        {
            _walking = true;
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

            if (LiftedItem != null)
                LiftedItem.SetAnimationWalking();
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
        #endregion

        #region 상태
        int _whenSuspended;

        bool _blinking;
        public bool IsBlinking
        {
            get { return _blinking; }
        }

        int _endBlinkDate;

        public void Blink(int duration)
        {
            int blinkDelay = 50;

            _blinking = true;
            _tunicSprite.SetBlinking(blinkDelay);

            if (duration == 0)
                _endBlinkDate = 0;  // 끝나지 않습니다
            else
                _endBlinkDate = Core.Now + duration;
        }

        public void StopBlink()
        {
            _blinking = false;
            _endBlinkDate = 0;

            _tunicSprite.SetBlinking(0);
        }

        bool _walking;

        public bool IsWalking
        {
            get { return _walking; }
        }
        #endregion

        public CarriedItem LiftedItem { get; set; }
    }
}
