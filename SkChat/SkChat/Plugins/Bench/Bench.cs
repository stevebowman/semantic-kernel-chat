using System.Diagnostics;

namespace SkChat.Bench;

public static class Bench
{
    public static async Task<double> TimeAsync(
        Func<Task> work, int warmup = 2, int samples = 5)
    {
        for (int i = 0; i < warmup; i++) await work();
        var sw = new Stopwatch();
        double total = 0;
        for (int i = 0; i < samples; i++)
        {
            sw.Restart();
            await work();
            sw.Stop();
            total += sw.Elapsed.TotalMilliseconds;
        }
        return total / samples;
    }
}