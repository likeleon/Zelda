using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Engine;

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

    class Npc : Detector
    {
        public Npc(
            Game game, 
            string name,
            Layer layer,
            Point xy,
            NpcSubtype subtype,
            string spriteName,
            Direction4 initialDirection,
            string behaviorString)
            : base(CollisionMode.Facing | CollisionMode.Overlapping, name, layer, xy, new Size(0, 0))
        {
            _subtype = subtype;

            InitializeSprite(spriteName, initialDirection);
            Size = new Size(16, 16);
            Origin = new Point(8, 13);
            Direction = initialDirection;

            SetDrawnInYOrder(subtype == NpcSubtype.UsualNpc);

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
            {
                Debug.Die("Invalid behavior string for NPC '{0}': '{1'}".F(name, behaviorString));
            }
        }

        readonly NpcSubtype _subtype;
        readonly NpcBehavior _behavior;
        readonly string _itemName;
        readonly string _dialogToShow;

        public override EntityType Type
        {
            get { return EntityType.Npc; }
        }
        public bool IsSolid
        {
            get { return _subtype != NpcSubtype.UsualNpc; }
        }

        public bool CanBeLifted
        {
            get { return HasSprite && Sprite.AnimationSetId == "entities/sign"; }
        }

        void InitializeSprite(string spriteName, Direction4 initialDirection)
        {
            if (!String.IsNullOrEmpty(spriteName))
            {
                CreateSprite(spriteName);
                if (initialDirection != Direction4.None)
                    Sprite.SetCurrentDirection(initialDirection);
            }
        }

        public override bool IsObstacleFor(MapEntity other)
        {
            return other.IsNpcObstacle(this);
        }

        public override bool IsNpcObstacle(Npc npc)
        {
            return (_subtype != NpcSubtype.UsualNpc || npc._subtype != NpcSubtype.UsualNpc);
        }

        public override void NotifyCollision(MapEntity entityOverlapping, CollisionMode collisionMode)
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

        public override bool NotifyActionCommandPressed()
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
                    Game.StartDialog(_dialogToShow, () => { });
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
                    Sound.Play("lift");
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

        public override void NotifyPositionChanged()
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

        public override void NotifyMovementFinished()
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
            Sprite = xmlData.Sprite.OptField(String.Empty);
            Behavior = xmlData.Behavior.OptField("map");
        }
    }

    public class NpcXmlData : EntityXmlData
    {
        public Direction4? Direction { get; set; }
        public NpcSubtype? Subtype { get; set; }
        public string Sprite { get; set; }
        public string Behavior { get; set; }
    }
}
