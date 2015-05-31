using System;
using System.Collections.Generic;
using System.Linq;

namespace Zelda.Game.Script
{
    public class ScriptTimer
    {
        readonly Timer _timer;

        [CLSCompliant(false)]
        public static ScriptTimer Start(object context, uint delay, Func<bool> callback)
        {
            return ScriptToCore.Call(() =>
            {
                object timerContext = context ?? ScriptContext.MainLoop.Game;

                Timer timer = new Timer(delay);
                AddTimer(timer, timerContext, callback.Invoke);

                if (delay == 0)
                    DoTimerCallback(timer);

                return new ScriptTimer(timer);
            });
        }

        ScriptTimer(Timer timer)
        {
            _timer = timer;
        }

        public void Stop()
        {
            ScriptToCore.Call(() => RemoveTimer(_timer));
        }

        #region 타이머 관리
        public delegate bool TimerCallback();

        class ScriptTimerData
        {
            TimerCallback _callback;
            public TimerCallback Callback
            {
                get { return _callback; }
                set { _callback = value; }
            }

            readonly object _context;
            public object Context
            {
                get { return _context; }
            }

            public ScriptTimerData(TimerCallback callback, object context)
            {
                _callback = callback;
                _context = context;
            }
        }

        static readonly Dictionary<Timer, ScriptTimerData> _timers = new Dictionary<Timer, ScriptTimerData>();
        static readonly List<Timer> _timersToRemove = new List<Timer>();

        internal static void AddTimer(Timer timer, object context, TimerCallback callback)
        {
            if (_timers.Values.Any(data => callback == data.Callback))
                throw new InvalidOperationException("Callback already used by a timer");

            if (_timers.ContainsKey(timer))
                throw new InvalidOperationException("Duplicate timer in the system");

            _timers.Add(timer, new ScriptTimerData(callback, context));
        }

        internal static void RemoveTimer(Timer timer)
        {
            if (_timers.ContainsKey(timer))
            {
                _timers[timer].Callback = null;
                _timersToRemove.Add(timer);
            }
        }

        internal static void DoTimerCallback(Timer timer)
        {
            if (!timer.IsFinished)
                throw new InvalidOperationException("This timer is still running");

            if (!_timers.ContainsKey(timer) || _timers[timer].Callback == null)
                return;

            bool repeat = CoreToScript.Call<bool>(_timers[timer].Callback.Invoke);
            if (repeat)
            {
                timer.ExpirationDate += timer.InitialDuration;
                if (timer.IsFinished)
                {
                    // Duration이 메인 루프 step 시간보다 작다면 바로 완료상태가 될 수 있습니다
                    DoTimerCallback(timer);
                }
            }
            else
            {
                _timers[timer].Callback = null;
                _timersToRemove.Add(timer);
            }
        }

        internal static void UpdateTimers()
        {
            // DoTimerCallback에서 _timers를 변경할 수 있기 때문에 이에 대한 복사본을 얻습니다
            var timersToUpdate = new Dictionary<Timer, ScriptTimerData>(_timers);

            // 모든 유효한 타이머들을 갱신합니다
            foreach (var entry in timersToUpdate)
            {
                Timer timer = entry.Key;
                TimerCallback callback = entry.Value.Callback;
                if (callback != null)
                {
                    timer.Update();
                    if (timer.IsFinished)
                        DoTimerCallback(timer);
                }
            }

            // 삭제 예정 타이머들을 삭제합니다
            foreach (Timer timer in _timersToRemove)
            {
                if (_timers.ContainsKey(timer))
                    _timers.Remove(timer);
            }
            _timersToRemove.Clear();
        }

        internal static void DestroyTimers()
        {
            _timers.Clear();
        }

        // context와 관련된 모든 타이머들을 해제합니다
        internal static void RemoveTimers(object context)
        {
            foreach (var entry in _timers)
            {
                Timer timer = entry.Key;
                if (entry.Value.Context == context)
                {
                    entry.Value.Callback = null;
                    _timersToRemove.Add(timer);
                }
            }
        }
        #endregion
    }
}
