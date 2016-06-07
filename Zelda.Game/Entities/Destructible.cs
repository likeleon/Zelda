using System;
using System.ComponentModel;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public class Destructible : Detector
    {
        internal Treasure Treasure { get; set; }
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

        public override EntityType Type => EntityType.Destructible;

        internal override bool IsGroundModifier
        {
            get
            {
                return _modifiedGround != Ground.Wall &&
                       _modifiedGround != Ground.Empty &&
                       _modifiedGround != Ground.Traversable;
            }
        }

        internal string AnimationSetId { get; }
        internal bool IsWaitingForRegeneration => false;

        readonly Ground _modifiedGround;
        int _weight;
        bool _canBeCut;

        internal Destructible(string name, Layer layer, Point xy, string animationSetId, Treasure treasure, Ground modifiedGround)
            : base(name, layer, xy, new Size(16, 16))
        {
            _modifiedGround = modifiedGround;
            AnimationSetId = animationSetId;
            Treasure = treasure;
            DamageOnEnemies = 1;

            Origin = new Point(8, 13);
            CreateSprite(AnimationSetId);

            UpdateCollisionModes();
        }

        internal override bool IsObstacleFor(Entity other)
        {
            return ModifiedGround == Ground.Wall &&
                   other.IsDestructibleObstacle(this);
        }

        internal override void NotifyCollision(Entity entityOverlapping, CollisionMode collisionMode)
        {
            entityOverlapping.NotifyCollisionWithDestructible(this, collisionMode);
        }

        internal void NotifyCollisionWithHero(Hero hero, CollisionMode collisionMode)
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

        internal override bool NotifyActionCommandPressed()
        {
            var effect = CommandsEffects.ActionCommandEffect;
            
            if ((effect != ActionCommandEffect.Lift && effect != ActionCommandEffect.Look) ||
                Weight == -1)
                return false;

            if (Equipment.HasAbility(Ability.Lift, Weight))
            {
                int explosionDate = 0;
                Hero.StartLifting(new CarriedObject(
                    Hero,
                    this,
                    AnimationSetId,
                    DestructionSound,
                    DamageOnEnemies,
                    explosionDate));

                Core.Audio?.Play("lift");

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

    public class DestructibleData : EntityData
    {
        public override EntityType Type => EntityType.Destructible;

        [DefaultValue(null)]
        public string TreasureName { get; set; }

        [DefaultValue(1)]
        public int TreasureVariant { get; set; } = 1;

        [DefaultValue(null)]
        public string TreasureSavegameVariable { get; set; }

        public string Sprite { get; set; }

        [DefaultValue(null)]
        public string DestructionSound { get; set; }

        [DefaultValue(0)]
        public int Weight { get; set; }

        [DefaultValue(false)]
        public bool CanBeCut { get; set; }

        [DefaultValue(false)]
        public bool CanExplode { get; set; }

        [DefaultValue(false)]
        public bool CanRegenerate { get; set; }

        [DefaultValue(1)]
        public int DamageOnEnemies { get; set; } = 1;

        [DefaultValue(Ground.Wall)]
        public Ground Ground { get; set; } = Ground.Wall;
    }
}
