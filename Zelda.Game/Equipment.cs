using System;

namespace Zelda.Game
{
    class Equipment
    {
        public int MaxLife
        {
            get
            {
                return _savegame.GetInteger(Savegame.Key.MaxLife);
            }
            set
            {
                Debug.CheckAssertion(value >= 0, "Invalid life amount");

                _savegame.SetInteger(Savegame.Key.MaxLife, value);

                if (Life > MaxLife)
                    Life = MaxLife;
            }
        }

        public int Life
        {
            get
            {
                return _savegame.GetInteger(Savegame.Key.CurrentLife);
            }
            set
            {
                int life = Math.Max(0, Math.Min(MaxLife, value));
                _savegame.SetInteger(Savegame.Key.CurrentLife, life);
            }
        }

        public Equipment(Savegame saveGame)
        {
            _savegame = saveGame;
        }

        readonly Savegame _savegame;
        public Savegame Savegame
        {
            get { return _savegame; }
        }

        public Game Game
        {
            get { return _savegame.Game; }
        }

        public void Update()
        {
            Game game = _savegame.Game;
            if (game == null)
                return;

            bool gameSuspended = game.IsSuspended;
            if (_suspended != gameSuspended)
                SetSuspended(gameSuspended);
        }

        bool _suspended;
        public void SetSuspended(bool suspended)
        {
            _suspended = suspended;
        }

        #region 기본 제공 능력들
        public bool HasAbility(Ability ability, int level = 1)
        {
            return GetAbility(ability) >= level;
        }

        public void SetAbility(Ability ability, int level)
        {
            _savegame.SetInteger(GetAbilitySavegameVariable(ability), level);

            if (Game != null)
            {
                if (ability == Ability.Tunic ||
                    ability == Ability.Sword ||
                    ability == Ability.Shield)
                {
                    // 영웅 스프라이트가 이 능력들에 의해 영향을 받습니다
                    Game.Hero.RebuildEquipment();
                }
            }
        }

        public int GetAbility(Ability ability)
        {
            return _savegame.GetInteger(GetAbilitySavegameVariable(ability));
        }

        Savegame.Key GetAbilitySavegameVariable(Ability ability)
        {
            switch (ability)
            {
                case Ability.Tunic:
                    return Savegame.Key.AbilityTunic;

                case Ability.Sword:
                    return Savegame.Key.AbilitySword;

                case Ability.Shield:
                    return Savegame.Key.AbilityShield;

                case Ability.Lift:
                    return Savegame.Key.AbilityLift;

                case Ability.Swim:
                    return Savegame.Key.AbilitySwim;

                case Ability.Run:
                    return Savegame.Key.AbilityRun;

                case Ability.DetectWeakWalls:
                    return Savegame.Key.AbilityDetectWeakWalls;
            }

            Debug.Die("Invalid ability");
            return (Savegame.Key)(-1);
        }
        #endregion

        public void RestoreAllLife()
        {
            Life = MaxLife;
        }
    }
}
