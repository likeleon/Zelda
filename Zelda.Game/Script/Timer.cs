using System;
using RawTimer = Zelda.Game.Timer;

namespace Zelda.Game.Script
{
    public class Timer
    {
        readonly RawTimer _rawTimer;

        [CLSCompliant(false)]
        public static Timer Start(object context, uint delay, Func<bool> callback)
        {
            return ScriptTools.ExceptionBoundaryHandle<Timer>(() =>
            {
                object timerContext = context ?? ScriptContext.MainLoop.Game;

                RawTimer timer = new RawTimer(delay);
                ScriptContext.AddTimer(timer, timerContext, callback.Invoke);

                if (delay == 0)
                    ScriptContext.DoTimerCallback(timer);

                return new Timer(timer);
            });
        }

        Timer(RawTimer rawTimer)
        {
            _rawTimer = rawTimer;
        }

        public void Stop()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                ScriptContext.RemoveTimer(_rawTimer);
            });
        }
    }
}
