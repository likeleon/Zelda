using System;
using Zelda.Game.Engine;

namespace Zelda.Game.Entities
{
    class Destructible : Detector
    {
        #region 생성
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
            _animationSetId = animationSetId;
            Treasure = treasure;
            DamageOnEnemies = 1;

            Origin = new Point(8, 13);
            CreateSprite(_animationSetId);

            UpdateCollisionModes();
        }
        #endregion

        #region MapEntity 속성 재정의
        public override EntityType Type
        {
            get { return EntityType.Destructible; }
        }

        readonly Ground _modifiedGround;
        public Ground ModifiedGround
        {
            get { return _modifiedGround; }
        }
        #endregion

        #region 상태
        public bool IsWaitingForRegeneration
        {
            get {  return false; }
        }
        #endregion

        #region 충돌
        public override bool IsObstacleFor(MapEntity other)
        {
            return _modifiedGround == Ground.Wall &&
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
        #endregion

        #region Destructible 고유 특성들
        readonly string _animationSetId;
        public string AnimationSetId
        {
            get { return _animationSetId; }
        }

        int _weight;
        public int Weight
        {
            get { return _weight; }
            set 
            { 
                _weight = value;
                UpdateCollisionModes();
            }
        }

        bool _canBeCut;
        public bool CanBeCut
        {
            get { return _canBeCut; }
            set
            {
                _canBeCut = value;
                UpdateCollisionModes();
            }
        }

        public Treasure Treasure { get; set; }
        public string DestructionSound { get; set; }
        public bool CanExplode { get; set; }
        public bool CanRegenerate { get; set; }
        public int DamageOnEnemies { get; set; }

        void UpdateCollisionModes()
        {
            SetCollisionModes(CollisionMode.None);

            if (ModifiedGround == Ground.Wall)
                AddCollisionMode(CollisionMode.Facing);
        }
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
    }
}
