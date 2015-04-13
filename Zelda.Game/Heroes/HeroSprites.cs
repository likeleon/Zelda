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

        public void SetAnimationDirection(int direction)
        {
            Debug.CheckAssertion(direction >= 0 && direction < 4, "Invalid direction for SetAnimationDirection");

            _tunicSprite.SetCurrentDirection(direction);
        }
    }
}
