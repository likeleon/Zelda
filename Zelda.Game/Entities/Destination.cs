using System.ComponentModel;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Destination : Entity
    {
        public override EntityType Type => EntityType.Destination;

        internal override bool CanBeObstacle => false;
        internal bool IsDefault { get; }

        internal Destination(string name, Layer layer, Point xy, Direction4 heroDirection, string spriteName, bool isDefault)
            : base(name, heroDirection, layer, xy, new Size(16, 16))
        {
            IsDefault = isDefault;
            
            Origin = new Point(8, 13);

            if (spriteName != null)
                CreateSprite(spriteName);
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
    }
}
