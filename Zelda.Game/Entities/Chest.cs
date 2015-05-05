using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    public enum ChestOpeningMethod
    {
        ByInteraction,
        ByInteractionIfSavegameVariable,
        ByInteractionIfItem
    }

    class Chest : Detector
    {        
        readonly Treasure _treasure;
        
        public Chest(
            string name, 
            Layer layer, 
            Point xy, 
            string spriteName, 
            Treasure treasure)
            : base(CollisionMode.Facing, name, layer, xy, new Size(16, 16))
        {
            _treasure = treasure;
            _open = treasure.IsFound;
            OpeningMethod = ChestOpeningMethod.ByInteraction;

            Sprite sprite = CreateSprite(spriteName);
            string animation = IsOpen ? "open" : "closed";
            sprite.SetCurrentAnimation(animation);

            Origin = new Point(Width / 2, Height - 3);

            SetDrawnInYOrder(sprite.MaxSize.Height > Height);
        }
        
        public override EntityType Type
        {
            get { return EntityType.Chest; }
        }

        bool _open;
        public bool IsOpen
        {
            get { return _open; }
        }

        public ChestOpeningMethod OpeningMethod { get; set; }
        public string OpeningCondition { get; set; }
        public bool OpeningConditionConsumed { get; set; }
        public string CannotOpenDialogId { get; set; }

        public override bool IsObstacleFor(MapEntity other)
        {
            return true;
        }
    }

    class ChestData : EntityData
    {
        public string TreasureName { get; set; }
        public int TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public ChestOpeningMethod OpeningMethod { get; set; }
        public string OpeningCondition { get; set; }
        public bool OpeningConditionConsumed { get; set; }
        public string CannotOpenDialog { get; set; }

        public ChestData(ChestXmlData xmlData)
            : base(EntityType.Chest, xmlData)
        {
            TreasureName = xmlData.TreasureName.OptField("");
            TreasureVariant = xmlData.TreasureVariant.OptField(1);
            TreasureSavegameVariable = xmlData.TreasureSavegameVariable.OptField("");
            Sprite = xmlData.Sprite.CheckField("");
            OpeningMethod = xmlData.OpeningMethod.OptField("OpeningMethod", ChestOpeningMethod.ByInteraction);
            OpeningCondition = xmlData.OpeningCondition.OptField("");
            OpeningConditionConsumed = xmlData.OpeningConditionConsumed.OptField(false);
            CannotOpenDialog = xmlData.CannotOpenDialog.OptField("");
        }
    }

    public class ChestXmlData : EntityXmlData
    {
        public string TreasureName { get; set; }
        public int? TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public ChestOpeningMethod? OpeningMethod { get; set; }
        public string OpeningCondition { get; set; }
        public bool? OpeningConditionConsumed { get; set; }
        public string CannotOpenDialog { get; set; }
    }
}
