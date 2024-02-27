using System;
using System.Threading;

namespace HashDog;

public class Watchdog
{
    public Timer timer;
    public TimeSpan duration;

    public Watchdog(TimeSpan duration, Timer timer)
    {
        this.duration = duration;
        this.timer = timer;
    }

    public void Stop()
    {
        timer.Dispose();
    }
}