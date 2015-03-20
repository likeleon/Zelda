using System;
using System.Collections.Generic;
using System.Linq;
using RawTimer = Zelda.Game.Timer;

namespace Zelda.Game.Script
{
    static partial class ScriptContext
    {
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

        static readonly Dictionary<RawTimer, ScriptTimerData> _timers = new Dictionary<RawTimer, ScriptTimerData>();
        static readonly List<RawTimer> _timersToRemove = new List<RawTimer>();

        internal static void AddTimer(RawTimer timer, object context, TimerCallback callback)
        {
            if (_timers.Values.Any(data => callback == data.Callback))
                throw new InvalidOperationException("Callback already used by a timer");

            if (_timers.ContainsKey(timer))
                throw new InvalidOperationException("Duplicate timer in the system");

            _timers.Add(timer, new ScriptTimerData(callback, context));
        }

        internal static void RemoveTimer(RawTimer timer)
        {
            if (_timers.ContainsKey(timer))
            {
                _timers[timer].Callback = null;
                _timersToRemove.Add(timer);
            }
        }

        internal static void DoTimerCallback(RawTimer timer)
        {
            if (!timer.IsFinished)
                throw new InvalidOperationException("This timer is still running");

            if (!_timers.ContainsKey(timer) || _timers[timer].Callback == null)
                return;

            bool repeat = ScriptTools.ExceptionBoundaryHandle<bool>(_timers[timer].Callback.Invoke);
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

        static void UpdateTimers()
        {
            // DoTimerCallback에서 _timers를 변경할 수 있기 때문에 이에 대한 복사본을 얻습니다
            var timersToUpdate = new Dictionary<RawTimer, ScriptTimerData>(_timers);
            
            // 모든 유효한 타이머들을 갱신합니다
            foreach (var entry in timersToUpdate)
            {
                RawTimer timer = entry.Key;
                TimerCallback callback = entry.Value.Callback;
                if (callback != null)
                {
                    timer.Update();
                    if (timer.IsFinished)
                        DoTimerCallback(timer);
                }
            }

            // 삭제 예정 타이머들을 삭제합니다
            foreach (RawTimer timer in _timersToRemove)
            {
                if (_timers.ContainsKey(timer))
                    _timers.Remove(timer);
            }
            _timersToRemove.Clear();
        }

        static void DestroyTimers()
        {
            _timers.Clear();
        }

        // context와 관련된 모든 타이머들을 해제합니다
        static void RemoveTimers(object context)
        {
            foreach (var entry in _timers)
            {
                RawTimer timer = entry.Key;
                if (entry.Value.Context == context)
                {
                    entry.Value.Callback = null;
                    _timersToRemove.Add(timer);
                }
            }
        }
    }
}
