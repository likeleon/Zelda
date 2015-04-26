using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class Destructible
    {
        #region 생성
        public Destructible(
            string name,
            Layer layer,
            Point xy,
            string animationSetId,
            Treasure treasure,
            Ground modifiedGround)
        {

        }
        #endregion

        #region MapEntity 재정의
        #endregion
    }

    class DestructibleData : EntityData
    {
        public string TreasureName { get; set; }
        public int TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public string DestructionSound { get; set; }
        public int Weight { get; set; }
        public bool CanBeCut { get; set; }
        public bool CanExplode { get; set; }
        public bool CanRegenerate { get; set; }
        public int DamageOnEnemies { get; set; }
        public string Ground { get; set; }

        public DestructibleData(DestructibleXmlData xmlData)
            : base(EntityType.Destructible, xmlData)
        {
            TreasureName = xmlData.TreasureName.OptField("");
            TreasureVariant = xmlData.TreasureVariant.OptField(1);
            TreasureSavegameVariable = xmlData.TreasureSavegameVariable.OptField("");
            Sprite = xmlData.Sprite.CheckField("Sprite");
            DestructionSound = xmlData.DestructionSound.OptField("");
            Weight = xmlData.Weight.OptField(0);
            CanBeCut = xmlData.CanBeCut.OptField(false);
            CanExplode = xmlData.CanExplode.OptField(false);
            CanRegenerate = xmlData.CanRegenerate.OptField(false);
            DamageOnEnemies = xmlData.DamageOnEnemies.OptField(1);
            Ground = xmlData.Ground.OptField("Wall");
        }
    }

    public class DestructibleXmlData : EntityXmlData
    {
        public string TreasureName { get; set; }
        public int? TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public string DestructionSound { get; set; }
        public int? Weight { get; set; }
        public bool? CanBeCut { get; set; }
        public bool? CanExplode { get; set; }
        public bool? CanRegenerate { get; set; }
        public int? DamageOnEnemies { get; set; }
        public string Ground { get; set; }
    }
}
