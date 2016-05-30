using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Explosion : Detector
    {
        public override EntityType Type => EntityType.Explosion;

        internal override bool CanBeObstacle => false;

        public Explosion(string name, Layer layer, Point xy, bool withDamage)
            : base(name, layer, xy, new Size(48, 48))
        {
            SetCollisionModes(CollisionMode.Sprite | CollisionMode.Overlapping);

            var sprite = CreateSprite("entities/explosion");
            sprite.EnablePixelCollisions();

            if (withDamage)
            {
                Size = new Size(48, 48);
                Origin = new Point(24, 24);
            }
        }

        internal override void Update()
        {
            base.Update();

            if (GetSprite()?.IsAnimationFinished == true)
                RemoveFromMap();
        }

        internal override void NotifySpriteFrameChanged(Sprite sprite, string animation, int frame)
        {
            if (frame == 1)
            {
                // 픽셀 단위가 아닌 충돌 검사도 합니다
                CheckCollisionWithDetectors();
            }
        }

        internal override void NotifyCollision(Entity otherEntity, Sprite thisSprite, Sprite otherSprite)
        {
            otherEntity.NotifyCollisionWithExplosion(this, otherSprite);
        }
    }
}
