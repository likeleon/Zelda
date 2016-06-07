using System;
using System.ComponentModel;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Entities
{
    public enum ChestOpeningMethod
    {
        ByInteraction,
        ByInteractionIfSavegameVariable,
        ByInteractionIfItem
    }

    public class Chest : Detector
    {        
        public override EntityType Type => EntityType.Chest;
        public bool IsOpen { get; private set; }

        internal ChestOpeningMethod OpeningMethod { get; set; }
        internal string OpeningCondition { get; set; }
        internal bool OpeningConditionConsumed { get; set; }
        internal string CannotOpenDialogId { get; set; }

        readonly Treasure _treasure;
        bool _treasureGiven;
        int _treasureDate;

        internal Chest(ChestData data, Game game)
            : base(data.Name, data.Layer, data.XY, new Size(16, 16))
        {
            OpeningMethod = data.OpeningMethod;
            OpeningCondition = data.OpeningCondition;
            OpeningConditionConsumed = data.OpeningConditionConsumed;
            CannotOpenDialogId = data.CannotOpenDialog;

            if (OpeningMethod == ChestOpeningMethod.ByInteractionIfItem)
            {
                if (!game.Equipment.ItemExists(OpeningCondition))
                    throw new Exception("Bad field 'OpeningCondition' (no such equipement item: '{0}'".F(OpeningCondition));

                var item = game.Equipment.GetItem(OpeningCondition);
                if (!item.IsSaved)
                    throw new Exception("Bad field 'OpeneingCondition' (equipment item '{0}' is not saved".F(OpeningCondition));
            }
            
            _treasure = new Treasure(game, data.TreasureName, data.TreasureVariant, data.TreasureSavegameVariable);
            IsOpen = _treasure.IsFound;
            _treasureGiven = IsOpen;
            OpeningMethod = ChestOpeningMethod.ByInteraction;

            SetCollisionModes(CollisionMode.Facing);

            var sprite = CreateSprite(data.Sprite);
            sprite.SetCurrentAnimation(IsOpen ? "open" : "closed");

            Origin = new Point(Width / 2, Height - 3);

            IsDrawnInYOrder = sprite.MaxSize.Height > Height;
        }

        internal override bool IsObstacleFor(Entity other) => true;

        internal override void NotifyCollision(Entity entityOverlapping, CollisionMode collisionMode)
        {
            if (!IsSuspended)
                entityOverlapping.NotifyCollisionWithChest(this);
        }

        internal override bool NotifyActionCommandPressed()
        {
            if (!IsEnabled ||
                !Hero.IsFree ||
                CommandsEffects.ActionCommandEffect == ActionCommandEffect.None)
                return false;

            if (CanOpen())
            {
                Core.Audio?.Play("chest_open");
                SetOpen(true);
                _treasureDate = Core.Now + 300;

                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
                Hero.StartFreezed();
            }

            return true;
        }

        internal override void Update()
        {
            if (IsOpen && !IsSuspended)
            {
                if (!_treasureGiven && _treasureDate != 0 && Core.Now >= _treasureDate)
                {
                    _treasureDate = 0;
                    _treasure.EnsureObtainable();
                    if (!_treasure.IsEmpty)
                    {
                        // 플레이어에게 보물을 줍니다
                        Hero.StartTreasure(_treasure, null);
                        _treasureGiven = true;
                    }
                    else
                    {
                        // 빈 보물 상자
                        if (_treasure.IsSaved)
                        {
                            // 보물이 찾아진 것으로 표시합니다
                            Savegame.SetBoolean(_treasure.SavegameVariable, true);
                        }

                        _treasureGiven = true;

                        Hero.StartFree();
                    }
                }
            }

            base.Update();
        }

        internal bool CanOpen()
        {
            switch (OpeningMethod)
            {
                case ChestOpeningMethod.ByInteraction:
                    return true;

                case ChestOpeningMethod.ByInteractionIfSavegameVariable:
                {
                    string requiredSavegameVariable = OpeningCondition;
                    if (string.IsNullOrEmpty(requiredSavegameVariable))
                        return false;

                    if (Savegame.IsBoolean(requiredSavegameVariable))
                        return Savegame.GetBoolean(requiredSavegameVariable);
                    else if (Savegame.IsInteger(requiredSavegameVariable))
                        return Savegame.GetInteger(requiredSavegameVariable) > 0;
                    else if (Savegame.IsString(requiredSavegameVariable))
                        return !String.IsNullOrEmpty(Savegame.GetString(requiredSavegameVariable));

                    return false;
                }

                case ChestOpeningMethod.ByInteractionIfItem:
                {
                    string requiredItemName = OpeningCondition;
                    if (String.IsNullOrEmpty(requiredItemName))
                        return false;

                    EquipmentItem item = Equipment.GetItem(requiredItemName);
                    return item.IsSaved &&
                           item.Variant > 0 &&
                           (!item.HasAmount || item.Amount > 0);
                }
                    
                default:
                    return false;
            }
        }

        public void SetOpen(bool open)
        {
            if (open == IsOpen)
                return;

            IsOpen = open;

            var sprite = GetSprite();
            if (open)
                sprite?.SetCurrentAnimation("open");
            else
            {
                sprite?.SetCurrentAnimation("closed");
                _treasureGiven = false;
            }
        }
    }

    public class ChestData : EntityData
    {
        public override EntityType Type => EntityType.Chest;

        [DefaultValue(null)]
        public string TreasureName { get; set; }

        [DefaultValue(1)]
        public int TreasureVariant { get; set; } = 1;

        [DefaultValue(null)]
        public string TreasureSavegameVariable { get; set; }

        public string Sprite { get; set; }

        [DefaultValue(ChestOpeningMethod.ByInteraction)]
        public ChestOpeningMethod OpeningMethod { get; set; } = ChestOpeningMethod.ByInteraction;

        [DefaultValue(null)]
        public string OpeningCondition { get; set; }

        [DefaultValue(false)]
        public bool OpeningConditionConsumed { get; set; }

        [DefaultValue(null)]
        public string CannotOpenDialog { get; set; }

        internal override void CreateEntity(Map map)
        {
            map.Entities.AddEntity(new Chest(this, map.Game));
        }
    }
}
