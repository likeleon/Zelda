
namespace Zelda.Game
{
    public enum ActionCommandEffect
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

    public enum SwordCommandEffect
    {
        None,
        Sword,
        Count
    }

    public enum PauseCommandEffect
    {
        None,
        Pause,
        Return,
        Count
    }

    class CommandsEffects
    {
        #region 액션
        ActionCommandEffect _actionCommandEffect = ActionCommandEffect.None;
        public ActionCommandEffect ActionCommandEffect
        {
            get { return _actionCommandEffect; }
            set { _actionCommandEffect = value; }
        }

        bool _actionCommandEnabled;
        public bool ActionCommandEnabled
        {
            get { return _actionCommandEnabled; }
            set { _actionCommandEnabled = true; }
        }

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
                return _actionCommandEffect == ActionCommandEffect.Look ||
                       _actionCommandEffect == ActionCommandEffect.Open ||
                       _actionCommandEffect == ActionCommandEffect.Lift ||
                       _actionCommandEffect == ActionCommandEffect.Speak ||
                       _actionCommandEffect == ActionCommandEffect.Grab;
            }
        }
        #endregion
    }
}
