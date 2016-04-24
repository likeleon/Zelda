
namespace Zelda.Game
{
    enum ActionCommandEffect
    {
        None,
        Next,
        Look,
        Open,
        Lift,
        Throw,
        Grab,
        Speak,
        Swim,
        Count
    }

    enum SwordCommandEffect
    {
        None,
        Sword,
        Count
    }

    enum PauseCommandEffect
    {
        None,
        Pause,
        Return,
        Count
    }

    class CommandsEffects
    {
        public ActionCommandEffect ActionCommandEffect { get; set; } = ActionCommandEffect.None;
        public bool ActionCommandEnabled { get; set; }

        ActionCommandEffect _actionCommandEffectSaved = ActionCommandEffect.None;

        public void SaveActionCommandEffect()
        {
            _actionCommandEffectSaved = ActionCommandEffect;
        }

        public void RestoreActionCommandEffect()
        {
            ActionCommandEffect = _actionCommandEffectSaved;
        }

        public bool IsActionCommandActingOnFacingEntity
        {
            get
            {
                return ActionCommandEffect == ActionCommandEffect.Look ||
                       ActionCommandEffect == ActionCommandEffect.Open ||
                       ActionCommandEffect == ActionCommandEffect.Lift ||
                       ActionCommandEffect == ActionCommandEffect.Speak ||
                       ActionCommandEffect == ActionCommandEffect.Grab;
            }
        }
    }
}
