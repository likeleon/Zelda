using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    class Destructible : Detector
    {
        readonly Ground _modifiedGround;
        readonly ScriptDestructible _scriptDestructible;
        int _weight;
        bool _canBeCut;

        public string AnimationSetId { get; private set; }
        public bool IsWaitingForRegeneration { get { return false; } }
        public Treasure Treasure { get; set; }
        public string DestructionSound { get; set; }
        public bool CanExplode { get; set; }
        public bool CanRegenerate { get; set; }
        public int DamageOnEnemies { get; set; }

        public bool CanBeCut
        {
            get { return _canBeCut; }
            set
            {
                _canBeCut = value;
                UpdateCollisionModes();
            }
        }

        public int Weight
        {
            get { return _weight; }
            set
            {
                _weight = value;
                UpdateCollisionModes();
            }
        }

        public override Ground ModifiedGround
        {
            get
            {
                if (IsWaitingForRegeneration)
                    return Ground.Empty;
                
                return _modifiedGround;
            }
        }

        public override EntityType Type { get { return EntityType.Destructible; } }
        public override ScriptEntity ScriptEntity { get { return _scriptDestructible; } }

        public override bool IsGroundModifier
        {
            get
            {
                return _modifiedGround != Ground.Wall &&
                       _modifiedGround != Ground.Empty &&
                       _modifiedGround != Ground.Traversable;
            }
        }

        public Destructible(
            string name,
            Layer layer,
            Point xy,
            string animationSetId,
            Treasure treasure,
            Ground modifiedGround)
            : base((int)CollisionMode.None, name, layer, xy, new Size(16, 16))
        {
            _modifiedGround = modifiedGround;
            AnimationSetId = animationSetId;
            Treasure = treasure;
            DamageOnEnemies = 1;

            Origin = new Point(8, 13);
            CreateSprite(AnimationSetId);

            UpdateCollisionModes();

            _scriptDestructible = new ScriptDestructible(this);
        }

        public override bool IsObstacleFor(MapEntity other)
        {
            return ModifiedGround == Ground.Wall &&
                   other.IsDestructibleObstacle(this);
        }

        public override void NotifyCollision(MapEntity entityOverlapping, CollisionMode collisionMode)
        {
            entityOverlapping.NotifyCollisionWithDestructible(this, collisionMode);
        }

        public void NotifyCollisionWithHero(Hero hero, CollisionMode collisionMode)
        {
            if (Weight != -1 &&
                CommandsEffects.ActionCommandEffect == ActionCommandEffect.None)
            {
                if (Equipment.HasAbility(Ability.Lift, Weight))
                    CommandsEffects.ActionCommandEffect = ActionCommandEffect.Lift;
                else
                    CommandsEffects.ActionCommandEffect = ActionCommandEffect.Look;
            }
        }

        public override bool NotifyActionCommandPressed()
        {
            ActionCommandEffect effect = CommandsEffects.ActionCommandEffect;
            
            if ((effect != ActionCommandEffect.Lift && effect != ActionCommandEffect.Look) ||
                Weight == -1)
                return false;

            if (Equipment.HasAbility(Ability.Lift, Weight))
            {
                uint explosionDate = 0;
                Hero.StartLifting(new CarriedItem(
                    Hero,
                    this,
                    AnimationSetId,
                    DestructionSound,
                    DamageOnEnemies,
                    explosionDate));

                Sound.Play("lift");

                if (!CanRegenerate)
                    RemoveFromMap();
                else
                    throw new NotImplementedException();
            }
            else
            {
                // TODO: Grab 처리
            }

            return true;
        }

        void UpdateCollisionModes()
        {
            SetCollisionModes(CollisionMode.None);

            if (ModifiedGround == Ground.Wall)
                AddCollisionMode(CollisionMode.Facing);
        }
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
        public Ground Ground { get; set; }

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
            Ground = xmlData.Ground.OptField("Ground", Ground.Wall);
        }

        protected override EntityXmlData ExportXmlData()
        {
            var data = new DestructibleXmlData();
            if (!TreasureName.IsNullOrEmpty())
                data.TreasureName = TreasureName;
            if (TreasureVariant != 1)
                data.TreasureVariant = TreasureVariant;
            if (!TreasureSavegameVariable.IsNullOrEmpty())
                data.TreasureSavegameVariable = TreasureSavegameVariable;
            data.Sprite = Sprite;
            if (!DestructionSound.IsNullOrEmpty())
                data.DestructionSound = DestructionSound;
            if (Weight != 0)
                data.Weight = Weight;
            if (CanBeCut != false)
                data.CanBeCut = CanBeCut;
            if (CanExplode != false)
                data.CanExplode = CanExplode;
            if (CanRegenerate != false)
                data.CanRegenerate = CanRegenerate;
            if (DamageOnEnemies != 1)
                data.DamageOnEnemies = DamageOnEnemies;
            if (Ground != Ground.Wall)
                data.Ground = Ground;
            return data;
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
        public Ground? Ground { get; set; }

        public bool ShouldSerializeTreasureName() { return !TreasureName.IsNullOrEmpty(); }
        public bool ShouldSerializeTreasureVariant() { return TreasureVariant != -1; }
        public bool ShouldSerializeTreasureSavegameVariable() { return !TreasureSavegameVariable.IsNullOrEmpty(); }
        public bool ShouldSerializeDestructionSound() { return !DestructionSound.IsNullOrEmpty(); }
        public bool ShouldSerializeWeight() { return Weight != 0; }
        public bool ShouldSerializeCanBeCut() { return CanBeCut == true; }
        public bool ShouldSerializeCanExplode() { return CanExplode == true; }
        public bool ShouldSerializeCanRegenerate() { return CanRegenerate == true; }
        public bool ShouldSerializeDamageOnEnemies() { return DamageOnEnemies != -1; }
        public bool ShouldSerializeGround() { return Ground != Entities.Ground.Wall; }
    }
}
