using System;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    class Bomb : Detector
    {
        uint _explosionDate;

        public override EntityType Type
        {
            get { return EntityType.Bomb; }
        }

        public override bool CanBeObstacle
        {
            get { return false; }
        }

        public override bool IsDeepWaterObstacle
        {
            get { return false; }
        }

        public override bool IsHoleObstacle
        {
            get { return false; }
        }

        public override bool IsLavaObstacle
        {
            get { return false; }
        }

        public override bool IsPrickleObstacle
        {
            get { return false; }
        }

        public override bool IsLadderObstacle
        {
            get { return false; }
        }

        readonly ScriptBomb _scriptBomb;
        public override ScriptEntity ScriptEntity
        {
            get { return _scriptBomb; }
        }

        public Bomb(string name, Layer layer, Point xy)
            : base(CollisionMode.Facing, name, layer, xy, new Size(16, 16))
        {
            _explosionDate = MainLoop.Now + 6000;

            CreateSprite("entities/bomb");
            Sprite.EnablePixelCollisions();
            Size = new Size(16, 16);
            Origin = new Point(8, 13);
            SetDrawnInYOrder(true);
            OptimizationDistance = 0;   // 주인공이 멀리 있더라도 폭파될 수 있도록

            _scriptBomb = new ScriptBomb(this);
        }

        public override void NotifyCollision(MapEntity entityOverlapping, CollisionMode collisionMode)
        {
            entityOverlapping.NotifyCollisionWithBomb(this, collisionMode);
        }

        public override void NotifyPositionChanged()
        {
            base.NotifyPositionChanged();

            if (Hero.FacingEntity == this &&
                CommandsEffects.ActionCommandEffect == ActionCommandEffect.Lift &&
                !Hero.IsFacingPointIn(BoundingBox))
            {
                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
            }
        }

        public override bool NotifyActionCommandPressed()
        {
            ActionCommandEffect effect = CommandsEffects.ActionCommandEffect;
            
            if (effect == ActionCommandEffect.Lift &&
                Hero.FacingEntity == this &&
                Hero.IsFacingPointIn(BoundingBox))
            {
                Hero.StartLifting(new CarriedItem(Hero, this, "entities/bomb", String.Empty, 0, _explosionDate));
                Audio.Play("lift");
                RemoveFromMap();
                return true;
            }

            return false;
        }

        public override void NotifyCollisionWithExplosion(Explosion explosion, Sprite spriteOverlapping)
        {
            if (!IsBeingRemoved)
                Explode();
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended && WhenSuspended != 0)
            {
                uint diff = MainLoop.Now - WhenSuspended;
                _explosionDate += diff;
            }
        }

        public override void Update()
        {
            base.Update();

            if (IsSuspended)
                return;

            uint now = MainLoop.Now;
            if (now >= _explosionDate)
                Explode();
            else if (now >= _explosionDate - 1500 && Sprite.CurrentAnimation != "stopped_explosion_soon")
                Sprite.SetCurrentAnimation("stopped_explosion_soon");

            if (Movement != null && Movement.IsFinished)
                ClearMovement();

            CheckCollisionWithDetectors();
        }

        void Explode()
        {
            Entities.ScheduleAddEntity(new Explosion(String.Empty, Layer, CenterPoint, true));
            Audio.Play("explosion");
            RemoveFromMap();
        }
    }

    public class BombData : EntityData
    {
        public BombData()
            : base(EntityType.Bomb)
        {
        }
    }
}
