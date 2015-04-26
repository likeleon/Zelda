using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    // 다른 엔티티의 존재를 감지하는 능력이 있는 엔티티를 위한 추상 클래스
    abstract class Detector : MapEntity
    {
        readonly int _collisionModes;
        bool _layerIndependentCollisions;

        protected Detector(int collisionModes, string name, Layer layer, Point xy, Size size)
            : base(name, 0, layer, xy, size)
        {
            _collisionModes = collisionModes;
        }

        #region 속성
        public virtual bool LayerIndependentCollisions
        {
            get { return _layerIndependentCollisions; }
            set { _layerIndependentCollisions = value; }
        }
        #endregion
    }
}
