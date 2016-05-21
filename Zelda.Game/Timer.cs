using System;

namespace Zelda.Game
{
    public interface ITimerContext { }

    public class Timer
    {
        internal int ExpirationDate { get; set; }
        internal int InitialDuration { get; }
        internal bool IsFinished => Core.Now >= ExpirationDate;
        internal bool IsSuspended { get; private set; }

        int _whenSuspended;

        internal Timer(int duration)
        {
            ExpirationDate = Core.Now + duration;
            InitialDuration = duration;
        }

        internal void Update()
        {
            if (IsSuspended || IsFinished)
                return;
        }

        internal void SetSuspended(bool suspended)
        {
            if (IsSuspended == suspended)
                return;

            if (suspended)
                _whenSuspended = Core.Now;
            else if (_whenSuspended != 0)
                ExpirationDate += Core.Now - _whenSuspended;
        }
    }
}
