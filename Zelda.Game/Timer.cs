using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.Entities;
using Zelda.Game.Script;

namespace Zelda.Game
{
    public interface ITimerContext { };

    public class Timer
    {
        class TimerData
        {
            public ITimerContext Context { get; }
            public Func<bool> Callback { get; set; }

            public TimerData(ITimerContext context, Func<bool> callback)
            {
                Context = context;
                Callback = callback;
            }
        }

        internal int ExpirationDate { get; set; }
        internal int InitialDuration { get; }
        internal bool IsFinished => Core.Now >= ExpirationDate;
        internal bool IsSuspended { get; private set; }
        internal bool SuspendedWithMap
        {
            get { return _suspendedWithMap; }
            set
            {
                if (_suspendedWithMap == value)
                    return;

                _suspendedWithMap = value;
                if (IsSuspended && !SuspendedWithMap)
                    SetSuspended(false);
            }
        }

        static readonly Dictionary<Timer, TimerData> _timers = new Dictionary<Timer, TimerData>();
        static List<Timer> _timersToRemove = new List<Timer>();
        int _whenSuspended;
        bool _suspendedWithMap;

        public static Timer Start(ITimerContext context, int delay, Action callback)
        {
            return Start(context, delay, () =>
            {
                callback();
                return false;
            });
        }

        public static Timer Start(ITimerContext context, int delay, Func<bool> callback)
        {
            if (context == null)
            {
                if (Core.Game?.HasCurrentMap == true)
                    context = Core.Game.CurrentMap.ScriptMap;
                else
                    context = ScriptMain.Current;
            }

            var timer = new Timer(delay);
            AddTimer(timer, context, callback);

            if (delay == 0)
                DoTimerCallback(timer);

            return timer;
        }

        static void AddTimer(Timer timer, ITimerContext context, Func<bool> callback)
        {
            if (_timers.ContainsKey(timer))
                throw new InvalidOperationException("Duplicate timer in the system");

            _timers.Add(timer, new TimerData(context, callback));

            if (Core.Game != null)
                return;

            if (context is Map || context is MapEntity || context is EquipmentItem)
            {
                var initiallySuspended = false;
                if (context is MapEntity)
                {
                    var entity = context as MapEntity;
                    initiallySuspended = entity.IsSuspended || !entity.IsEnabled;
                }
                else
                {
                    timer.SuspendedWithMap = true;
                    initiallySuspended = Core.Game.IsDialogEnabled;
                }

                timer.SetSuspended(initiallySuspended);
            }
        }

        static void DoTimerCallback(Timer timer)
        {
            if (!timer.IsFinished)
                throw new InvalidOperationException("This timer is still running");

            if (!_timers.ContainsKey(timer) || _timers[timer].Callback == null)
                return;

            var repeat = _timers[timer].Callback();
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

        public static void StopAll(ITimerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            RemoveTimers(context);
        }

        static void RemoveTimers(ITimerContext context)
        {
            foreach (var kvp in _timers.Where(x => x.Value.Context == context))
            {
                kvp.Value.Callback = null;
                _timersToRemove.Add(kvp.Key);
            }
        }

        static void RemoveTimer(Timer timer)
        {
            if (!_timers.ContainsKey(timer))
                return;

            _timers[timer].Callback = null;
            _timersToRemove.Add(timer);
        }

        internal static void DestroyTimers()
        {
            _timers.Clear();
        }

        internal static void SetEntityTimersSuspended(MapEntity entity, bool suspended)
        {
            foreach (var kvp in _timers.Where(t => t.Value.Context == entity))
                kvp.Key.SetSuspended(suspended);
        }

        internal static void UpdateTimers()
        {
            // DoTimerCallback에서 _timers를 변경할 수 있기 때문에 이에 대한 복사본을 얻습니다
            var timersToUpdate = new Dictionary<Timer, TimerData>(_timers);

            // 모든 유효한 타이머들을 갱신합니다
            foreach (var timer in timersToUpdate.Where(x => x.Value.Callback != null).Select(x => x.Key))
            {
                timer.Update();
                if (timer.IsFinished)
                    DoTimerCallback(timer);
            }

            // 삭제 예정 타이머들을 삭제합니다
            foreach (var timer in _timersToRemove)
            {
                if (_timers.ContainsKey(timer))
                    _timers.Remove(timer);
            }
            _timersToRemove.Clear();
        }

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

        public void Stop()
        {
            RemoveTimer(this);
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
