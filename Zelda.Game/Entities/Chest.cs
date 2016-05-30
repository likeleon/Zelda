using System;
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

        internal Chest(string name, Layer layer, Point xy, string spriteName, Treasure treasure)
            : base(name, layer, xy, new Size(16, 16))
        {
            _treasure = treasure;
            IsOpen = treasure.IsFound;
            _treasureGiven = IsOpen;
            OpeningMethod = ChestOpeningMethod.ByInteraction;

            SetCollisionModes(CollisionMode.Facing);

            var sprite = CreateSprite(spriteName);
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

    class ChestData : EntityData
    {
        public string TreasureName { get; set; }
        public int TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public ChestOpeningMethod OpeningMethod { get; set; }
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
            OpeningMethod = xmlData.OpeningMethod.OptField("OpeningMethod", ChestOpeningMethod.ByInteraction);
            OpeningCondition = xmlData.OpeningCondition.OptField("");
            OpeningConditionConsumed = xmlData.OpeningConditionConsumed.OptField(false);
            CannotOpenDialog = xmlData.CannotOpenDialog.OptField("");
        }

        protected override EntityXmlData ExportXmlData()
        {
            var data = new ChestXmlData();
            if (!TreasureName.IsNullOrEmpty())
                data.TreasureName = TreasureName;
            if (TreasureVariant != 1)
                data.TreasureVariant = TreasureVariant;
            if (!TreasureSavegameVariable.IsNullOrEmpty())
                data.TreasureSavegameVariable = TreasureSavegameVariable;
            data.Sprite = Sprite;
            if (OpeningMethod != ChestOpeningMethod.ByInteraction)
                data.OpeningMethod = OpeningMethod;
            if (!OpeningCondition.IsNullOrEmpty())
                data.OpeningCondition = OpeningCondition;
            if (OpeningConditionConsumed)
                data.OpeningConditionConsumed = OpeningConditionConsumed;
            if (!CannotOpenDialog.IsNullOrEmpty())
                data.CannotOpenDialog = CannotOpenDialog;
            return data;
        }
    }

    public class ChestXmlData : EntityXmlData
    {
        public string TreasureName { get; set; }
        public int? TreasureVariant { get; set; }
        public string TreasureSavegameVariable { get; set; }
        public string Sprite { get; set; }
        public ChestOpeningMethod? OpeningMethod { get; set; }
        public string OpeningCondition { get; set; }
        public bool? OpeningConditionConsumed { get; set; }
        public string CannotOpenDialog { get; set; }

        public bool ShouldSerializeTreasureName() { return !TreasureName.IsNullOrEmpty(); }
        public bool ShouldSerializeTreasureVariant() { return TreasureVariant != 1; }
        public bool ShouldSerializeTreasureSavegameVariable() { return !TreasureSavegameVariable.IsNullOrEmpty(); }
        public bool ShouldSerializeOpeningMethod() { return OpeningMethod != ChestOpeningMethod.ByInteraction; }
        public bool ShouldSerializeOpeningCondition() { return !OpeningCondition.IsNullOrEmpty(); }
        public bool ShouldSerializeOpeningConditionConsumed() { return OpeningConditionConsumed == true; }
        public bool ShouldSerializeCannotOpenDialog() { return !CannotOpenDialog.IsNullOrEmpty(); }
    }
}
