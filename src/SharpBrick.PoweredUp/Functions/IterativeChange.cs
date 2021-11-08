using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.Functions;

public abstract class IterativeChange<T>
{
    private readonly ILogger<IterativeChange<T>> _logger;
    private readonly Stopwatch _stopwatch;

    protected abstract T Function(int idx);
    protected abstract Task ChangeAsync(T value, CancellationTokenSource cts);

    public IterativeChange(ILogger<IterativeChange<T>> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    protected async Task IterativeExecuteAsync(int iterations, int delayInMilliseconds, CancellationTokenSource cts = default)
    {
        _logger.LogInformation($"Start iterative change over {iterations} steps");
        _stopwatch.Start();

        for (int idx = 0; idx < iterations; idx++)
        {
            _logger.LogInformation($"+ Iteration: {idx}");
            var startTime = _stopwatch.ElapsedMilliseconds;
            if (cts?.IsCancellationRequested ?? false)
            {
                _logger.LogInformation("+ Cancelled (before Actor)");

                break;
            }

            var value = Function(idx);

            _logger.LogInformation($"+ Value: {value}");

            await ChangeAsync(value, cts);

            if (cts?.IsCancellationRequested ?? false)
            {
                _logger.LogInformation("+ Cancelled (after Actor)");

                break;
            }
            var endTime = _stopwatch.ElapsedMilliseconds;

            var currentDuration = endTime - startTime;
            var waiting = delayInMilliseconds - currentDuration;

            _logger.LogInformation($"+ Waiting {delayInMilliseconds}ms ({currentDuration} already elapsed; {waiting} to go; walltime: {endTime})");

            if (waiting > 0 && waiting <= int.MaxValue)
            {
                await Task.Delay((int)waiting);
            }
            else
            {
                _logger.LogError($"+ IO Time exceeded Waiting Time. Reducing Step Count or Increasing Delay Time might help.");
            }
        }

        _stopwatch.Stop();

        _logger.LogInformation("Finished iterative change");
    }
}
