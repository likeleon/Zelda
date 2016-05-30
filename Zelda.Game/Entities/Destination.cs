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

            if (!spriteName.IsNullOrEmpty())
                CreateSprite(spriteName);
        }
    }

    class DestinationData : EntityData
    {
        public Direction4 Direction { get; set; }
        public string Sprite { get; set; }
        public bool Default { get; set; }

        public DestinationData(DestinationXmlData xmlData)
            : base(EntityType.Destination, xmlData)
        {
            Direction = xmlData.Direction.CheckField("Direction");
            Sprite = xmlData.Sprite.OptField("");
            Default = xmlData.Default.OptField(false);
        }

        protected override EntityXmlData ExportXmlData()
        {
            var data = new DestinationXmlData();
            data.Direction = Direction;
            if (!Sprite.IsNullOrEmpty())
                data.Sprite = Sprite;
            if (Default)
                data.Default = true;
            return data;
        }
    }

    public class DestinationXmlData : EntityXmlData
    {
        public Direction4? Direction { get; set; }
        public string Sprite { get; set; }
        public bool? Default { get; set; }

        public bool ShouldSerializeSprite() { return !Sprite.IsNullOrEmpty(); }
        public bool ShouldSerializeDefault() { return Default == true; }
    }
}
