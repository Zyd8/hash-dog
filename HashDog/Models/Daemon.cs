using System;
using System.Threading;

namespace HashDog;

public class Daemon
{
    public Timer timer;
    public TimeSpan duration;

    public Daemon(TimeSpan duration, Timer timer)
    {
        this.duration = duration;
        this.timer = timer;
    }

    public void Stop()
    {
        timer.Dispose();
    }
}