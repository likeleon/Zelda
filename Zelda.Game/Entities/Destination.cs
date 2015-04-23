using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class Destination : MapEntity
    {
        public override EntityType Type
        {
            get { return EntityType.Destination; }
        }

        public override bool CanBeObstacle
        {
            get { return false; }
        }

        readonly bool _isDefaultDestination;
        public bool IsDefault
        {
            get { return _isDefaultDestination; }
        }

        public Destination(
            string name,
            Layer layer,
            Point xy,
            int heroDirection,
            string spriteName,
            bool isDefault)
            : base(name, heroDirection, layer, xy, new Size(16, 16))
        {
            _isDefaultDestination = isDefault;
            
            Origin = new Point(8, 13);

            if (!String.IsNullOrEmpty(spriteName))
                CreateSprite(spriteName);
        }
    }

    class DestinationData : EntityData
    {
        public int Direction { get; set; }
        public string Sprite { get; set; }
        public bool Default { get; set; }

        public DestinationData(DestinationXmlData xmlData)
            : base(EntityType.Destination, xmlData)
        {
            Direction = xmlData.Direction.CheckField("Direction");
            Sprite = xmlData.Sprite.OptField("");
            Default = xmlData.Default.OptField(false);
        }
    }

    public class DestinationXmlData : EntityXmlData
    {
        public int? Direction { get; set; }
        public string Sprite { get; set; }
        public bool? Default { get; set; }
    }
}
