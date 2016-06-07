using System.ComponentModel;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Destination : Entity
    {
        public override EntityType Type => EntityType.Destination;

        internal override bool CanBeObstacle => false;
        internal bool IsDefault { get; }

        internal Destination(DestinationData data)
            : base(data.Name, data.Direction, data.Layer, data.XY, new Size(16, 16))
        {
            IsDefault = data.Default;
            
            Origin = new Point(8, 13);

            if (data.Sprite != null)
                CreateSprite(data.Sprite);
        }
    }

    public class DestinationData : EntityData
    {
        public override EntityType Type => EntityType.Destination;

        public Direction4 Direction { get; set; }

        [DefaultValue(null)]
        public string Sprite { get; set; }

        [DefaultValue(false)]
        public bool Default { get; set; }

        internal override void CreateEntity(Map map)
        {
            map.Entities.AddEntity(new Destination(this));
        }
    }
}
