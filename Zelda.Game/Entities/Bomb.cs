using System;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Bomb : Detector
    {
        public override EntityType Type => EntityType.Bomb;

        internal override bool CanBeObstacle => false;
        internal override bool IsDeepWaterObstacle => false;
        internal override bool IsHoleObstacle => false;
        internal override bool IsLavaObstacle => false;
        internal override bool IsPrickleObstacle => false;
        internal override bool IsLadderObstacle => false;

        int _explosionDate = Core.Now + 6000;

        internal Bomb(string name, Layer layer, Point xy)
            : base(name, layer, xy, new Size(16, 16))
        {
            SetCollisionModes(CollisionMode.Facing);
            var sprite = CreateSprite("entities/bomb");
            sprite.EnablePixelCollisions();
            Size = new Size(16, 16);
            Origin = new Point(8, 13);
            IsDrawnInYOrder = true;
        }

        internal override void NotifyCollision(Entity entityOverlapping, CollisionMode collisionMode)
        {
            entityOverlapping.NotifyCollisionWithBomb(this, collisionMode);
        }

        internal override void NotifyPositionChanged()
        {
            base.NotifyPositionChanged();

            if (Hero.FacingEntity == this &&
                CommandsEffects.ActionCommandEffect == ActionCommandEffect.Lift &&
                !Hero.IsFacingPointIn(BoundingBox))
            {
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
        }

        internal override bool NotifyActionCommandPressed()
        {
            if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.Lift &&
                Hero.FacingEntity == this &&
                Hero.IsFacingPointIn(BoundingBox))
            {
                Hero.StartLifting(new CarriedObject(Hero, this, "entities/bomb", null, 0, _explosionDate));
                Core.Audio?.Play("lift");
                RemoveFromMap();
                return true;
            }

            return false;
        }

        internal override void NotifyCollisionWithExplosion(Explosion explosion, Sprite spriteOverlapping)
        {
            if (!IsBeingRemoved)
                Explode();
        }

        internal override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended && WhenSuspended != 0)
            {
                int diff = Core.Now - WhenSuspended;
                _explosionDate += diff;
            }
        }

        internal override void Update()
        {
            base.Update();

            if (IsSuspended)
                return;

            if (Core.Now >= _explosionDate)
                Explode();
            else if (Core.Now >= _explosionDate - 1500)
            {
                var sprite = GetSprite();
                if (sprite != null && sprite.CurrentAnimation != "stopped_explosion_soon")
                    sprite.SetCurrentAnimation("stopped_explosion_soon");
            }

            if (Movement?.IsFinished == true)
                ClearMovement();

            CheckCollisionWithDetectors();
        }

        void Explode()
        {
            Entities.ScheduleAddEntity(new Explosion(String.Empty, Layer, CenterPoint, true));
            Core.Audio?.Play("explosion");
            RemoveFromMap();
        }
    }

    public class BombData : EntityData
    {
        public BombData()
            : base(EntityType.Bomb)
        {
        }
    }
}
