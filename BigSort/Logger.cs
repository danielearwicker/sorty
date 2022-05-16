namespace BigSort;

using System.Diagnostics;

public static class Logger
{
    private static readonly Stopwatch _timer = new Stopwatch();

    static Logger() => _timer.Start();

    static long _throttleTime;

    public static void Log(string message, bool throttle)
    {
        var seconds = _timer.ElapsedMilliseconds / 1000;

        if (!throttle || seconds > _throttleTime)
        {
            _throttleTime = seconds;

            Console.WriteLine($"{seconds}: {message}");
        }
    }
}
