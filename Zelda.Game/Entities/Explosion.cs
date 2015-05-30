using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class Explosion : Detector
    {
        public override EntityType Type
        {
            get { return EntityType.Explosion; }
        }

        public override bool CanBeObstacle
        {
            get { return false; }
        }

        public Explosion(string name, Layer layer, Point xy, bool withDamage)
            : base(CollisionMode.Sprite | CollisionMode.Overlapping, name, layer, xy, new Size(48, 48))
        {
            CreateSprite("entities/explosion");

            OptimizationDistance = 2000;
            Sprite.EnablePixelCollisions();
            if (withDamage)
            {
                Size = new Size(48, 48);
                Origin = new Point(24, 24);
            }
        }

        #region 상태
        public override void Update()
        {
            base.Update();

            if (Sprite.IsAnimationFinished)
                RemoveFromMap();
        }

        public override void NotifySpriteFrameChanged(Sprite sprite, string animation, int frame)
        {
            if (frame == 1)
            {
                // 픽셀 단위가 아닌 충돌 검사도 합니다
                CheckCollisionWithDetectors();
            }
        }
        #endregion

        #region 충돌
        public override void NotifyCollision(MapEntity otherEntity, Sprite thisSprite, Sprite otherSprite)
        {
            otherEntity.NotifyCollisionWithExplosion(this, otherSprite);
        }
        #endregion
    }
}
