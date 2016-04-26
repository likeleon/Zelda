using System;
using Zelda.Game.LowLevel;

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
        public Game Game { get; private set; }
        public TransitionDirection Direction { get; }
        public bool IsSuspended { get; private set; }
        public uint WhenSuspended { get; private set; }

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

        public virtual bool NeedsPreviousSurface { get { return false; } }
        public abstract bool IsStarted { get; }
        public abstract bool IsFinished { get; }

        Surface _previousSurface;

        protected Transition(TransitionDirection direction)
        {
            Direction = direction;
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
            if (suspended != IsSuspended)
            {
                IsSuspended = suspended;
                if (suspended)
                    WhenSuspended = Framework.Now;
                NotifySuspended(suspended);
            }
        }

        public abstract void Start();
        public abstract void Update();
        public abstract void Draw(Surface dstSurface);
        protected abstract void NotifySuspended(bool suspended);
    }
}
