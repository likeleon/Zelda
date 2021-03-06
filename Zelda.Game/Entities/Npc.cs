﻿using System;
using System.ComponentModel;
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
        public bool CanBeLifted => GetSprite()?.AnimationSetId == "entities/sign";

        readonly NpcSubtype _subtype;
        readonly NpcBehavior _behavior;
        readonly string _itemName;
        readonly string _dialogToShow;

        internal Npc(NpcData data)
            : base(data.Name, data.Layer, data.XY, new Size(0, 0))
        {
            _subtype = data.Subtype;

            SetCollisionModes(CollisionMode.Facing | CollisionMode.Overlapping); 
            InitializeSprite(data.Sprite, data.Direction);
            Size = new Size(16, 16);
            Origin = new Point(8, 13);
            Direction = data.Direction;

            IsDrawnInYOrder = _subtype == NpcSubtype.UsualNpc;

            if (data.Behavior == "map")
                _behavior = NpcBehavior.MapScript;
            else if (data.Behavior.StartsWith("item#"))
            {
                _behavior = NpcBehavior.ItemScript;
                _itemName = data.Behavior.Substring(5);
            }
            else if (data.Behavior.StartsWith("dialog#"))
            {
                _behavior = NpcBehavior.Dialog;
                _dialogToShow = data.Behavior.Substring(7);
            }
            else
                throw new Exception("Invalid behavior string for NPC '{0}': '{1'}".F(data.Name, data.Behavior));
        }

        void InitializeSprite(string spriteName, Direction4 initialDirection)
        {
            if (spriteName == null)
                return;

            var sprite = CreateSprite(spriteName);
            if (initialDirection != Direction4.None)
                sprite.SetCurrentDirection(initialDirection);
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

            var effect = CommandsEffects.ActionCommandEffect;
            CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;

            var sprite = GetSprite();

            if (_subtype == NpcSubtype.UsualNpc)
            {
                var direction = (Direction4)(((int)Hero.AnimationDirection + 2) % 4);
                sprite?.SetCurrentDirection(direction);
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
                    Hero.StartLifting(new CarriedObject(
                        Hero,
                        this,
                        sprite?.AnimationSetId ?? "stopped",
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
                var sprite = GetSprite();
                if (Movement != null && sprite != null)
                {
                    // NPC가 이동 중입니다
                    if (sprite.CurrentAnimation != "walking")
                        sprite.SetCurrentAnimation("walking");

                    sprite.SetCurrentDirection(Movement.GetDisplayedDirection4());
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
                GetSprite()?.SetCurrentAnimation("stopped");
        }
    }

    public class NpcData : EntityData
    {
        public override EntityType Type => EntityType.Npc;

        public Direction4 Direction { get; set; }
        public NpcSubtype Subtype { get; set; }
        
        [DefaultValue(null)]
        public string Sprite { get; set; }

        [DefaultValue("map")]
        public string Behavior { get; set; } = "map";

        internal override void CreateEntity(Map map)
        {
            map.Entities.AddEntity(new Npc(this));
        }
    }
}
