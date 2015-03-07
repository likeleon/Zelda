using System.ComponentModel;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Timer
    {
        [Description("타이머가 완료되는 시각")]
        uint _expirationDate;
        public uint ExpirationDate
        {
            get { return _expirationDate; }
            set
            {
                _expirationDate = value;
                _finished = EngineSystem.Now >= _expirationDate;
            }
        }

        [Description("초기 지속 시간")]
        readonly uint _duration;
        public uint InitialDuration
        {
            get { return _duration; }
        }

        private bool _finished;
        public bool IsFinished
        {
            get { return _finished; }
        }

        public Timer(uint duration)
        {
            _expirationDate = EngineSystem.Now + duration;
            _duration = duration;
            _finished = (EngineSystem.Now >= _expirationDate);
        }

        public void Update()
        {
            if (IsFinished)
                return;

            uint now = EngineSystem.Now;
            _finished = (EngineSystem.Now >= _expirationDate);
        }
    }
}
