using System;
using Microsoft.Xna.Framework;

namespace Playground
{
    public class GameTimerEventArgs : EventArgs
    {
        public object State { get; private set; }

        public GameTimerEventArgs(object state)
        {
            State = state;
        }
    }

    public class GameTimer
    {
        private float _elapsed;
        private long _interval;
        private bool _runOnce;
        private object _state;

        public bool IsRunning;
        public event EventHandler<GameTimerEventArgs> Tick;

        public GameTimer()
        {
            _elapsed = 0.0f;
            _interval = 0;
            _runOnce = false;
            IsRunning = false;
        }

        private void Reset()
        {
            _elapsed = 0;
        }

        public void Once(long interval, object state)
        {
            _interval = interval;
            _runOnce = true;
            IsRunning = true;
            _state = state;
        }

        public void Start(long interval)
        {
            Start(interval, null);
        }

        public void Start(long interval, object state)
        {
            _interval = interval;
            _runOnce = false;
            IsRunning = true;
            _state = state;
        }

        public void Stop()
        {
            _elapsed = 0.0f;
            _interval = 0;
            _runOnce = false;
            IsRunning = false;
            _state = null;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsRunning)
            {
                return;
            }

            _elapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!(_elapsed >= _interval))
            {
                return;
            }

            var eventArgs = new GameTimerEventArgs(_state);

            if (_runOnce)
            {
                Stop();
            }
            else
            {
                Reset();
            }

            // invoke callback event
            Tick?.Invoke(this, eventArgs);
        }
    }
}
