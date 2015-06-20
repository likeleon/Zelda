﻿using System.ComponentModel;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Timer
    {
        uint _expirationDate;
        uint _whenSuspended;

        public uint ExpirationDate
        {
            get { return _expirationDate; }
            set
            {
                _expirationDate = value;
                IsFinished = EngineSystem.Now >= _expirationDate;
            }
        }

        public uint InitialDuration { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsSuspended { get; private set; }

        public Timer(uint duration)
        {
            _expirationDate = EngineSystem.Now + duration;
            InitialDuration = duration;
            IsFinished = (EngineSystem.Now >= _expirationDate);
        }

        public void Update()
        {
            if (IsSuspended || IsFinished)
                return;

            var now = EngineSystem.Now;
            IsFinished = (now >= _expirationDate);
        }

        public void SetSuspended(bool suspended)
        {
            if (IsSuspended == suspended)
                return;

            var now = EngineSystem.Now;

            if (suspended)
                _whenSuspended = now;
            else if (_whenSuspended != 0)
                _expirationDate += now - _whenSuspended;
        }
    }
}
