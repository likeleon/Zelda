using System;
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
        string _tunicSpriteId;
        public string TunicSpriteId
        {
            get { return _tunicSpriteId; }
        }

        string GetDefaultTunicSpriteId()
        {
            int tunicLevel = _equipment.GetAbility(Ability.Tunic);
            return "hero/tunic" + tunicLevel;
        }

        public void SetTunicSpriteId(string spriteId)
        {
            if (spriteId == _tunicSpriteId)
                return;

            _tunicSpriteId = spriteId;

            string animation = null;
            int direction = -1;
            if (_tunicSprite != null)
            {
                animation = _tunicSprite.CurrentAnimation;
                direction = _tunicSprite.CurrentDirection;
                _tunicSprite = null;
            }

            _tunicSprite = new Sprite(spriteId);
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
            int animationDirection = -1;
            if (_tunicSprite != null)
            {
                // 방향 저장
                animationDirection = _tunicSprite.CurrentDirection;
            }

            // 영웅 몸체
            SetTunicSpriteId(GetDefaultTunicSpriteId());

            // 방향 복구
            if (animationDirection != -1)
                SetAnimationDirection(animationDirection);
        }

        public void DrawOnMap()
        {
            Map map = _hero.Map;

            int x = _hero.DisplayedXY.X;
            int y = _hero.DisplayedXY.Y;

            map.DrawSprite(_tunicSprite, x, y);
        }

        public void Update()
        {
            _tunicSprite.Update();
        }

        #region 애니메이션
        public int AnimationDirection
        {
            get { return _tunicSprite.CurrentDirection; }
        }

        static readonly int[,] _animationDirections = new int[,]
        {
            { 0, -1 },    // right
            { 0,  1 },    // right-up: right or up
            { 1, -1 },    // up
            { 2,  1 },    // left-up: left or up
            { 2, -1 },    // left
            { 2,  3 },    // left-down: left or down
            { 3, -1 },    // down
            { 0,  3 }     // right-down: right or down
        };

        public int GetAnimationDirection(int keysDirection, int realMovementDirection)
        {
            int result = 0;
            if (keysDirection == -1)
            {
                // 유효하지 않은 방향키 조합인 경우: 스프라이트 방향 변경 없음
                result = -1;
            }
            else if (keysDirection % 2 == 0)
            {
                // 네 방향 중 한 방향만 입력한 경우: 해당 방향을 스프라이트에게 부여
                result = keysDirection / 2;
            }
            // 이동이 대각선 방향인 경우: 두 방향 중 하나를 선택해야 합니다
            else if (_animationDirections[realMovementDirection, 1] == AnimationDirection)
            {
                // 이미 스프라이트 방향이라면 두 번째 방향을 선택합니다
                result = _animationDirections[realMovementDirection, 1];
            }
            else
            {
                // 그렇지 않다면 첫 번째 방향을 선택합니다
                result = _animationDirections[realMovementDirection, 0];
            }
            return result;
        }

        public void SetAnimationDirection(int direction)
        {
            Debug.CheckAssertion(direction >= 0 && direction < 4, "Invalid direction for SetAnimationDirection");

            _tunicSprite.SetCurrentDirection(direction);
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

        void SetTunicAnimation(string animation)
        {
            _tunicSprite.SetCurrentAnimation(animation);
        }
        #endregion

        #region 상태
        bool _walking;

        public bool IsWalking
        {
            get { return _walking; }
        }
        #endregion
    }
}
