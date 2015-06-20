using System;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game.Entities
{
    public enum ChestOpeningMethod
    {
        ByInteraction,
        ByInteractionIfSavegameVariable,
        ByInteractionIfItem
    }

    class Chest : Detector
    {        
        readonly Treasure _treasure;
        bool _treasureGiven;
        uint _treasureDate;
        readonly ScriptChest _scriptChest;
        
        public Chest(
            string name, 
            Layer layer, 
            Point xy, 
            string spriteName, 
            Treasure treasure)
            : base(CollisionMode.Facing, name, layer, xy, new Size(16, 16))
        {
            _treasure = treasure;
            _open = treasure.IsFound;
            _treasureGiven = _open;
            OpeningMethod = ChestOpeningMethod.ByInteraction;

            Sprite sprite = CreateSprite(spriteName);
            string animation = IsOpen ? "open" : "closed";
            sprite.SetCurrentAnimation(animation);

            Origin = new Point(Width / 2, Height - 3);

            SetDrawnInYOrder(sprite.MaxSize.Height > Height);

            _scriptChest = new ScriptChest(this);
        }
        
        public override EntityType Type
        {
            get { return EntityType.Chest; }
        }

        public override ScriptEntity ScriptEntity
        {
            get { return _scriptChest; }
        }

        bool _open;
        public bool IsOpen
        {
            get { return _open; }
        }

        public ChestOpeningMethod OpeningMethod { get; set; }
        public string OpeningCondition { get; set; }
        public bool OpeningConditionConsumed { get; set; }
        public string CannotOpenDialogId { get; set; }

        public override bool IsObstacleFor(MapEntity other)
        {
            return true;
        }

        public override void NotifyCollision(MapEntity entityOverlapping, CollisionMode collisionMode)
        {
            if (!IsSuspended)
                entityOverlapping.NotifyCollisionWithChest(this);
        }

        public override bool NotifyActionCommandPressed()
        {
            if (!IsEnabled ||
                !Hero.IsFree ||
                CommandsEffects.ActionCommandEffect == ActionCommandEffect.None)
                return false;

            if (CanOpen())
            {
                Sound.Play("chest_open");
                SetOpen(true);
                _treasureDate = EngineSystem.Now + 300;

                CommandsEffects.ActionCommandEffect = ActionCommandEffect.None;
                Hero.StartFreezed();
            }

            return true;
        }

        public override void Update()
        {
            if (IsOpen && !IsSuspended)
            {
                if (!_treasureGiven && _treasureDate != 0 && EngineSystem.Now >= _treasureDate)
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

        public bool CanOpen()
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
            if (open == _open)
                return;

            _open = open;
            if (open)
                Sprite.SetCurrentAnimation("open");
            else
            {
                Sprite.SetCurrentAnimation("closed");
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
    }
}
