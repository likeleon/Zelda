﻿using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public abstract class Detector : Entity
    {
        public virtual bool LayerIndependentCollisions { get; set; }

        internal override bool IsDetector => true;

        CollisionMode _collisionModes = CollisionMode.None;

        internal protected Detector(string name, Layer layer, Point xy, Size size)
            : base(name, 0, layer, xy, size)
        {
        }


        internal override void NotifyBeingRemoved()
        {
            base.NotifyBeingRemoved();

            if (Hero.FacingEntity == this)
                Hero.FacingEntity = null;
        }

        internal override void NotifyPositionChanged()
        {
            Map.CheckCollisionFromDetector(this);
            base.NotifyPositionChanged();
        }

        internal override void NotifyLayerChanged()
        {
            Map.CheckCollisionFromDetector(this);
            
            base.NotifyLayerChanged();
        }

        internal void CheckCollision(Entity entity)
        {
            if (entity == this)
                return;

            if (!HasLayerIndependentCollisions && Layer != entity.Layer)
                return;

            if (HasCollisionMode(CollisionMode.Facing) && TestCollisionFacingPoint(entity))
            {
                if (entity.FacingEntity == null)
                    entity.FacingEntity = this;
                NotifyCollision(entity, CollisionMode.Facing);
            }
        }

        internal void CheckCollision(Entity entity, Sprite sprite)
        {
            if (HasCollisionMode(CollisionMode.Sprite) &&
                entity != this &&
                (HasLayerIndependentCollisions || Layer == entity.Layer))
            {
                foreach (Sprite thisSprite in Sprites)
                    if (thisSprite.TestCollision(sprite, X, Y, entity.X, entity.Y))
                        NotifyCollision(entity, thisSprite, sprite);
            }
        }

        internal virtual bool NotifyActionCommandPressed() => false;
        internal virtual bool NotifyInteractionWithItem(EquipmentItem item) => false;
        internal virtual bool StartMovementByHero() => false;
        internal virtual void StopMovementByHero() { }

        protected bool HasCollisionMode(CollisionMode collisionMode)
        {
            return (_collisionModes & collisionMode) != 0;
        }

        protected void SetCollisionModes(CollisionMode collisionModes)
        {
            if ((collisionModes & CollisionMode.Sprite) != 0)
                EnablePixelCollisions();

            _collisionModes = collisionModes;
        }

        protected void AddCollisionMode(CollisionMode collisionMode)
        {
            SetCollisionModes(_collisionModes | collisionMode);
        }

        protected void EnablePixelCollisions() => Sprites.Do(s => s.EnablePixelCollisions());

        protected bool TestCollisionFacingPoint(Entity entity) => entity.IsFacingPointIn(BoundingBox);

        internal virtual void NotifyCollision(Entity entityOverlapping, CollisionMode collisionMode) { }
        internal virtual void NotifyCollision(Entity otherEntity, Sprite thisSprite, Sprite otherSprite) { }
    }
}
