using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    // 다른 엔티티의 존재를 감지하는 능력이 있는 엔티티를 위한 추상 클래스
    abstract class Detector : MapEntity
    {
        CollisionMode _collisionModes;
        bool _layerIndependentCollisions;

        protected Detector(CollisionMode collisionModes, string name, Layer layer, Point xy, Size size)
            : base(name, 0, layer, xy, size)
        {
            _collisionModes = collisionModes;
        }

        public override bool IsDetector
        {
            get { return true; }
        }

        #region 파괴
        public override void NotifyBeingRemoved()
        {
            base.NotifyBeingRemoved();

            if (Hero.FacingEntity == this)
                Hero.FacingEntity = null;
        }
        #endregion

        #region 속성
        public virtual bool LayerIndependentCollisions
        {
            get { return _layerIndependentCollisions; }
            set { _layerIndependentCollisions = value; }
        }
        #endregion

        #region 위치
        public override void NotifyPositionChanged()
        {
            Map.CheckCollisionFromDetector(this);
            base.NotifyPositionChanged();
        }

        public override void NotifyLayerChanged()
        {
            Map.CheckCollisionFromDetector(this);
            
            base.NotifyLayerChanged();
        }
        #endregion

        #region 충돌 함수들
        public void CheckCollision(MapEntity entity)
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

        public void CheckCollision(MapEntity entity, Sprite sprite)
        {
            if (HasCollisionMode(CollisionMode.Sprite) &&
                entity != this &&
                (HasLayerIndependentCollisions || Layer == entity.Layer))
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region 기타
        public virtual bool NotifyActionCommandPressed()
        {
            return false;
        }

        public virtual bool NotifyInteractionWithItem(EquipmentItem item)
        {
            return false;
        }

        public virtual bool StartMovementByHero()
        {
            return false;
        }
        
        public virtual void StopMovementByHero()
        {
        }
        #endregion

        #region Protected - Detector 특성들
        protected bool HasCollisionMode(CollisionMode collisionMode)
        {
            return (_collisionModes & collisionMode) != 0;
        }

        protected void SetCollisionModes(CollisionMode collisionModes)
        {
            _collisionModes = collisionModes;
        }

        protected void AddCollisionMode(CollisionMode collisionMode)
        {
            SetCollisionModes(_collisionModes | collisionMode);
        }
        #endregion

        #region Protected - 특수화된 충돌 함수들
        protected bool TestCollisionFacingPoint(MapEntity entity)
        {
            return entity.IsFacingPointIn(BoundingBox);
        }
        #endregion

        #region Protected - 충돌이 감지되었을 때 불리는 함수들
        public virtual void NotifyCollision(MapEntity entityOverlapping, CollisionMode collisionMode)
        {
        }
        #endregion
    }
}
