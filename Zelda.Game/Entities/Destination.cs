
namespace Zelda.Game.Entities
{
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
