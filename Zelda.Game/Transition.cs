using System;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    public enum TransitionStyle
    {
        Immediate,  // 두 표면간 전이 없음
        Fade,       // Fade-out과 fade-in 효과
        Scrolling   // 두 맵간 스크롤
    }

    public enum TransitionDirection
    {
        Opening,
        Closing

    }

    abstract class Transition
    {
        readonly TransitionDirection _direction;
        Surface _previousSurface;
        bool _suspended;
        uint _whenSuspended;
        
        public Game Game { get; private set; }

        public TransitionDirection Direction
        {
            get { return _direction; }
        }

        public Surface PreviousSurface
        {
            protected get { return _previousSurface; }
            set
            {
                Debug.CheckAssertion(value == null || Direction != TransitionDirection.Closing,
                    "Cannot show a previous surface with an closing transition effect");
                _previousSurface = value;
            }
        }

        public virtual bool NeedsPreviousSurface
        {
            get { return false; }
        }

        public bool IsSuspended
        {
            get { return _suspended; }
        }

        public uint WhenSuspended
        {
            get { return _whenSuspended; }
        }

        protected Transition(TransitionDirection direction)
        {
            _direction = direction;
        }

        public Transition Create(TransitionStyle style, TransitionDirection direction, Surface dstSurface, Game game = null)
        {
            Transition transition = null;

            switch (style)
            {
                case TransitionStyle.Immediate:
                    throw new NotImplementedException();

                case TransitionStyle.Fade:
                    transition = new TransitionFade(direction, dstSurface);
                    break;

                case TransitionStyle.Scrolling:
                    throw new NotImplementedException();
            }

            transition.Game = game;
            return transition;
        }

        public void SetSuspended(bool suspended)
        {
            if (suspended != _suspended)
            {
                _suspended = suspended;
                if (suspended)
                    _whenSuspended = EngineSystem.Now;
                NotifySuspended(suspended);
            }
        }

        public abstract bool IsStarted { get; }
        public abstract bool IsFinished { get; }
        public abstract void Start();
        public abstract void Update();
        public abstract void Draw(Surface dstSurface);
        protected abstract void NotifySuspended(bool suspended);
    }
}
