using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptHero : ScriptEntity
    {
        readonly Hero _hero;

        public Direction4 Direction
        {
            get { return _hero.AnimationDirection; }
        }

        internal ScriptHero(Hero hero)
            : base(hero)
        {
            _hero = hero;
        }
    }
}
