﻿using System;
using System.Collections.Generic;
using System.Linq;
using Zelda.Game.LowLevel;

namespace Zelda.Game.Movements
{
    class PixelMovement : Movement
    {
        public PixelMovement(string trajectoryString, int delay, bool loop, bool ignoreObstacles)
            : base(ignoreObstacles)
        {
            Delay = delay;
            _loop = loop;
            SetTrajectory(trajectoryString);
        }

        #region 속성
        public int Delay { get; set; }

        bool _loop;
        public bool Loop
        {
            get { return _loop; }
        }

        public void SetLoop(bool loop)
        {
            _loop = loop;
            if (IsFinished && loop)
                Restart();
        }

        List<Point> _trajectory = new List<Point>();
        public IEnumerable<Point> Trajectory
        {
            get { return _trajectory; }
        }

        string _trajectoryString;

        public void SetTrajectory(string trajectoryString)
        {
            _trajectory.Clear();
            try
            {
                int[] numbers = trajectoryString.Split((string[])null, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => Convert.ToInt32(s))
                                                .ToArray();
                for (int i = 0; i < numbers.Length; i += 2)
                    _trajectory.Add(new Point(numbers[i], numbers[i + 1]));
            }
            catch (Exception e)
            {
                Debug.Die("Invalid trajectory string: '{0}': {1}".F(trajectoryString, e.Message));
            }
            _trajectoryString = trajectoryString;
            
            Restart();
        }

        public void SetTrajectory(IEnumerable<Point> trajectory)
        {
            _trajectory = trajectory.ToList();
            _trajectoryString = String.Empty;   // 필요할 때 계산됩니다

            Restart();
        }
        #endregion

        #region 상태
        bool _finished;
        public override bool IsFinished
        {
            get { return _finished; }
        }

        public override bool IsStarted
        { 
            get { return !IsFinished; } 
        }

        public int Length
        {
            get { return _trajectory.Count; }
        }

        int _nbStepsDone;
        IEnumerator<Point> _trajectoryEnumerator;
        int _nextMoveDate;

        void Restart()
        {
            if (Length == 0)
                _finished = true;
            else
            {
                _nbStepsDone = 0;
                _finished = false;
                _trajectoryEnumerator = _trajectory.GetEnumerator();

                if (_nextMoveDate == 0)
                    _nextMoveDate = Core.Now;
                _nextMoveDate += Delay;

                NotifyMovementChanged();
            }
        }

        public override void Update()
        {
            int now = Core.Now;

            while (now >= _nextMoveDate &&
                   !IsSuspended &&
                   !IsFinished &&
                   (Entity == null || Entity.Movement == this))
            {
                Point oldXy = XY;
                MakeNextStep();
            }

            base.Update();
        }

        void MakeNextStep()
        {
            bool success = false;
            Point dxy = _trajectoryEnumerator.Current;

            if (!TestCollisionWithObstacles(dxy))
            {
                TranslateXY(dxy);
                success = true;
            }

            bool isLast = !_trajectoryEnumerator.MoveNext();

            if (isLast)
            {
                if (_loop)
                    _trajectoryEnumerator = _trajectory.GetEnumerator();
                else
                    _finished = true;
            }

            if (!IsFinished)
                _nextMoveDate += Delay;

            int stepIndex = _nbStepsDone;
            ++_nbStepsDone;
            NotifyStepDone(stepIndex, success);
        }

        protected virtual void NotifyStepDone(int stepIndex, bool success)
        {
        }

        public override void SetSuspended(bool suspended)
        {
            base.SetSuspended(suspended);

            if (!suspended &&
                WhenSuspended != 0 &&
                _nextMoveDate != 0)
            {
                _nextMoveDate += Core.Now - WhenSuspended;
            }
        }
        #endregion
    }
}
