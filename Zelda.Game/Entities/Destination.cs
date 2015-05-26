using System;
using Zelda.Game.Engine;
using Zelda.Game.Script;

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

        readonly ScriptDestination _scriptDestination;
        public override ScriptEntity ScriptEntity
        {
            get { return _scriptDestination; }
        }

        public Destination(
            string name,
            Layer layer,
            Point xy,
            Direction4 heroDirection,
            string spriteName,
            bool isDefault)
            : base(name, heroDirection, layer, xy, new Size(16, 16))
        {
            _isDefaultDestination = isDefault;
            
            Origin = new Point(8, 13);

            if (!String.IsNullOrEmpty(spriteName))
                CreateSprite(spriteName);

            _scriptDestination = new ScriptDestination(this);
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
    }

    public class DestinationXmlData : EntityXmlData
    {
        public Direction4? Direction { get; set; }
        public string Sprite { get; set; }
        public bool? Default { get; set; }
    }
}
