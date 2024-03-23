using System;
using System.Threading;

namespace HashDog
{
    public class Daemon
    {
        private TimeSpan target;
        private Timer? timer;

        public Daemon(TimeSpan target)
        {
            this.target = target;
        }

        public void Start(TimerCallback callback)
        {
            timer = new Timer(callback, null, target, target);
        }

        public void Stop()
        {
            timer?.Dispose();
        }
    }
}
