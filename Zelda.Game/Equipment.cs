using System;

namespace Zelda.Game
{
    class Equipment
    {
        public int MaxLife
        {
            get
            {
                return _saveGame.GetInteger(SaveGame.Key.MaxLife);
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Invalid life amount");

                _saveGame.SetInteger(SaveGame.Key.MaxLife, value);

                if (Life > MaxLife)
                    Life = MaxLife;
            }
        }

        public int Life
        {
            get
            {
                return _saveGame.GetInteger(SaveGame.Key.CurrentLife);
            }
            set
            {
                int life = Math.Max(0, Math.Min(MaxLife, value));
                _saveGame.SetInteger(SaveGame.Key.CurrentLife, life);
            }
        }

        readonly SaveGame _saveGame;

        public Equipment(SaveGame saveGame)
        {
            _saveGame = saveGame;
        }

        public void RestoreAllLife()
        {
            Life = MaxLife;
        }
    }
}
