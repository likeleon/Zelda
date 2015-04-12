using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    public class Chest : MapEntity
    {
        public enum OpeningMethod
        {
            ByInteraction,
            ByInteractionIfSavegameVariable,
            ByInteractionIfItem
        }

        public Chest(
            string name, 
            Layer layer, 
            Point xy, 
            string spriteName, 
            Treasure treasure)
            : base(name, )
        {

        }
    }

    class ChestData : EntityData
    {
        public string TreasureName { get; set; }
        public int TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public Chest.OpeningMethod OpeningMethod { get; set; }
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
            OpeningMethod = xmlData.OpeningMethod.OptField(Chest.OpeningMethod.ByInteraction);
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
        public Chest.OpeningMethod? OpeningMethod { get; set; }
        public string OpeningCondition { get; set; }
        public bool? OpeningConditionConsumed { get; set; }
        public string CannotOpenDialog { get; set; }
    }
}
