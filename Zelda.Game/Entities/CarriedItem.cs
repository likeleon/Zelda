using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class CarriedItem : MapEntity
    {
        public enum Behavior
        {
            Throw,
            Destroy,
            Keep
        }

        public CarriedItem(
            Hero hero,
            MapEntity originalEntity,
            string animationSetId,
            string destructionSoundId,
            int damageOnEnemies,
            uint explosionDate)
            : base("", Direction4.Right, hero.Layer, new Point(0, 0), new Size(0, 0))
        {
            _hero = hero;
            IsBeingLifted = true;

            Direction4 direction = hero.AnimationDirection;
            if ((int)direction % 2 == 0)
                XY = new Point(originalEntity.X, hero.Y);
            else
                XY = new Point(hero.X, originalEntity.Y);
            Origin = originalEntity.Origin;
            Size = originalEntity.Size;
            SetDrawnInYOrder(true);

            CreateSprite(animationSetId);
            Sprite.SetCurrentAnimation("stopped");
        }

        public override EntityType Type
        {
            get { return EntityType.CarriedItem; }
        }

        public override bool CanBeObstacle
        {
            get { return false; }
        }

        #region 게임 데이터
        readonly Hero _hero;
        #endregion

        #region 상태
        public bool IsBeingLifted { get; private set; }
        #endregion
    }
}
