using System;
using System.Threading;

namespace HashDog
{
    public class Daemon
    {
        private TimeSpan duration;
        private Timer? timer;

        public Daemon(TimeSpan duration)
        {
            this.duration = duration;
        }

        public void Start(TimerCallback callback)
        {
            timer = new Timer(callback, null, TimeSpan.Zero, duration);
        }

        public void Stop()
        {
            timer?.Dispose();
        }
    }
}
