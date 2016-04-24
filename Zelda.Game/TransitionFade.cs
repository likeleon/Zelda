using System;
using Zelda.Game.Lowlevel;

namespace Zelda.Game
{
    class TransitionFade : Transition
    {
        bool _finished;
        int _alphaStart;
        int _alphaLimit;
        int _alphaIncrement;
        int _alpha = -1;
        uint _nextFrameDate;
        Color _transitionColor = Color.Black;
        Surface _dstSurface;

        public uint Delay { get; set; }
        public bool IsColored { get; private set; }
        public Color Color 
        { 
            get { return _transitionColor; }
            set { _transitionColor = value; IsColored = true; }
        }

        public override bool IsStarted { get { return _alpha != -1 && !IsFinished; } }
        public override bool IsFinished { get { return _finished; } }

        public TransitionFade(TransitionDirection direction, Surface dstSurface)
            : base(direction)
        {
            _dstSurface = dstSurface;
            IsColored = true;

            if (direction == TransitionDirection.Closing)
            {
                _alphaStart = 256;
                _alphaLimit = 0;
                _alphaIncrement = -8;
            }
            else
            {
                _alphaStart = 0;
                _alphaLimit = 256;
                _alphaIncrement = 8;
            }

            Delay = 20;
        }

        public void ClearColor()
        {
            IsColored = false;
        }

        public override void Start()
        {
            _alpha = _alphaStart;
            _nextFrameDate = Engine.Now;
        }

        public override void Update()
        {
            if (!IsStarted || IsSuspended)
                return;

            while (Engine.Now >= _nextFrameDate && !_finished)
            {
                _alpha += _alphaIncrement;
                _nextFrameDate += Delay;

                _finished = (_alpha == _alphaLimit);
            }
        }

        public override void Draw(Surface dstSurface)
        {
            byte alphaImpl = (byte)Math.Min(_alpha, 255);

            if (!IsColored)
            {
                Debug.CheckAssertion(dstSurface.IsSoftwareDestination,
                    "Cannot apply fade transition: this surface is in read-only mode");
                _dstSurface.SetOpacity(alphaImpl);
            }
            else
            {
                byte r, g, b, a;
                _transitionColor.GetComponents(out r, out g, out b, out a);
                Color fadeColor = new Color(r, g, b, 255 - Math.Min(alphaImpl, a));

                dstSurface.FillWithColor(fadeColor);
            }

            _dstSurface = dstSurface;
        }

        protected override void NotifySuspended(bool suspended)
        {
            if (!suspended)
                _nextFrameDate += Engine.Now + WhenSuspended;
        }
    }
}
