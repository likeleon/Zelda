using System;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public enum NpcSubtype
    {
        GeneralizedNpc,
        UsualNpc
    }

    public enum NpcBehavior
    {
        Dialog,
        MapScript,
        ItemScript
    }

    public class Npc : Detector
    {
        public override EntityType Type => EntityType.Npc;
        public bool IsSolid => _subtype != NpcSubtype.UsualNpc;
        public bool CanBeLifted => HasSprite && Sprite.AnimationSetId == "entities/sign";

        readonly NpcSubtype _subtype;
        readonly NpcBehavior _behavior;
        readonly string _itemName;
        readonly string _dialogToShow;

        internal Npc(Game game, string name, Layer layer, Point xy, NpcSubtype subtype, string spriteName, Direction4 initialDirection, string behaviorString)
            : base(CollisionMode.Facing | CollisionMode.Overlapping, name, layer, xy, new Size(0, 0))
        {
            _subtype = subtype;

            InitializeSprite(spriteName, initialDirection);
            Size = new Size(16, 16);
            Origin = new Point(8, 13);
            Direction = initialDirection;

            IsDrawnInYOrder = subtype == NpcSubtype.UsualNpc;

            if (behaviorString == "map")
                _behavior = NpcBehavior.MapScript;
            else if (behaviorString.StartsWith("item#"))
            {
                _behavior = NpcBehavior.ItemScript;
                _itemName = behaviorString.Substring(5);
            }
            else if (behaviorString.StartsWith("dialog#"))
            {
                _behavior = NpcBehavior.Dialog;
                _dialogToShow = behaviorString.Substring(7);
            }
            else
                throw new Exception("Invalid behavior string for NPC '{0}': '{1'}".F(name, behaviorString));
        }

        void InitializeSprite(string spriteName, Direction4 initialDirection)
        {
            if (!spriteName.IsNullOrEmpty())
            {
                CreateSprite(spriteName);
                if (initialDirection != Direction4.None)
                    Sprite.SetCurrentDirection(initialDirection);
            }
        }

        internal override bool IsObstacleFor(Entity other) => other.IsNpcObstacle(this);
        internal override bool IsNpcObstacle(Npc npc) => (_subtype != NpcSubtype.UsualNpc || npc._subtype != NpcSubtype.UsualNpc);
        internal override bool IsHeroObstacle(Hero hero) => true;

        internal override void NotifyCollision(Entity entityOverlapping, CollisionMode collisionMode)
        {
            if (collisionMode == CollisionMode.Facing && entityOverlapping.IsHero)
            {
                Hero hero = entityOverlapping as Hero;
                if (CommandsEffects.ActionCommandEffect == ActionCommandEffect.None && hero.IsFree)
                {
                    if (_subtype == NpcSubtype.UsualNpc ||
                        Direction == Direction4.None ||
                        hero.IsFacingDirection4((Direction4)(((int)Direction + 2) % 4)))
                    {
                        CommandsEffects.ActionCommandEffect = (_subtype == NpcSubtype.UsualNpc) ?
                            ActionCommandEffect.Speak : ActionCommandEffect.Look;
                    }
                    else if (CanBeLifted && Equipment.HasAbility(Ability.Lift))
                        CommandsEffects.ActionCommandEffect = ActionCommandEffect.Lift;
                }
            }
            else if (collisionMode == CollisionMode.Overlapping && entityOverlapping.Type == EntityType.Fire)
            {
                throw new NotImplementedException();
            }
        }

        internal override bool NotifyActionCommandPressed()
        {
            if (!Hero.IsFree || CommandsEffects.ActionCommandEffect == ActionCommandEffect.None)
                return false;

            ActionCommandEffect effect = CommandsEffects.ActionCommandEffect;
            CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;

            if (_subtype == NpcSubtype.UsualNpc)
            {
                Direction4 direction = (Direction4)(((int)Hero.AnimationDirection + 2) % 4);
                Sprite.SetCurrentDirection(direction);
            }

            if (effect != ActionCommandEffect.Lift)
            {
                if (_behavior == NpcBehavior.Dialog)
                    Game.StartDialog(_dialogToShow, null, null);
                else
                    CallScriptHeroInteraction();
                return true;
            }
            else
            {
                if (Equipment.HasAbility(Ability.Lift))
                {
                    Hero.StartLifting(new CarriedItem(
                        Hero,
                        this,
                        Sprite.AnimationSetId,
                        "stone",
                        2,
                        0));
                    Core.Audio?.Play("lift");
                    RemoveFromMap();
                    return true;
                }
            }
            return false;
        }

        void CallScriptHeroInteraction()
        {
            throw new NotImplementedException();
        }

        internal override void NotifyPositionChanged()
        {
            base.NotifyPositionChanged();

            if (_subtype == NpcSubtype.UsualNpc)
            {
                if (Movement != null)
                {
                    // NPC가 이동 중입니다
                    if (Sprite.CurrentAnimation != "walking")
                        Sprite.SetCurrentAnimation("walking");

                    Direction4 direction4 = Movement.GetDisplayedDirection4();
                    Sprite.SetCurrentDirection(direction4);
                }

                if (Hero.FacingEntity == this &&
                    CommandsEffects.ActionCommandEffect == ActionCommandEffect.Speak &&
                    !Hero.IsFacingPointIn(BoundingBox))
                {
                    CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
                }
            }
        }

        internal override void NotifyMovementFinished()
        {
            base.NotifyMovementFinished();

            if (_subtype == NpcSubtype.UsualNpc)
                Sprite.SetCurrentAnimation("stopped");
        }
    }

    class NpcData : EntityData
    {
        public Direction4 Direction { get; set; }
        public NpcSubtype Subtype { get; set; }
        public string Sprite { get; set; }
        public string Behavior { get; set; }

        public NpcData(NpcXmlData xmlData)
            : base(EntityType.Npc, xmlData)
        {
            Direction = xmlData.Direction.CheckField("Direction");
            Subtype = xmlData.Subtype.CheckField("Subtype");
            Sprite = xmlData.Sprite.OptField(string.Empty);
            Behavior = xmlData.Behavior.OptField("map");
        }

        protected override EntityXmlData ExportXmlData()
        {
            var data = new NpcXmlData();
            data.Direction = Direction;
            data.Subtype = Subtype;
            if (!Sprite.IsNullOrEmpty())
                data.Sprite = Sprite;
            if (Behavior != "map")
                data.Behavior = Behavior;
            return data;
        }
    }

    public class NpcXmlData : EntityXmlData
    {
        public Direction4? Direction { get; set; }
        public NpcSubtype? Subtype { get; set; }
        public string Sprite { get; set; }
        public string Behavior { get; set; }

        public bool ShouldSerializeSprite() { return !Sprite.IsNullOrEmpty(); }
        public bool ShouldSerializeBehavior() { return Behavior != "map"; }
    }
}
