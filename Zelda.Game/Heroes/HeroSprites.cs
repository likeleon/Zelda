using Zelda.Game.Engine;
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
        }

        public void DrawOnMap()
        {
            // TODO: 영웅 스프라이트 그리기
        }
    }
}
