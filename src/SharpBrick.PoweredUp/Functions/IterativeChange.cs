using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.Functions
{
    public abstract class IterativeChange<T>
    {
        private readonly ILogger<IterativeChange<T>> _logger;

        protected abstract T Function(int idx);
        protected abstract Task ChangeAsync(T value, CancellationTokenSource cts);

        public IterativeChange(ILogger<IterativeChange<T>> logger)
        {
            _logger = logger;
        }

        protected async Task IterativeExecuteAsync(int iterations, int delayInMilliseconds, CancellationTokenSource cts = default)
        {
            _logger.LogInformation($"Start iterative change over {iterations} steps");

            for (int idx = 0; idx < iterations; idx++)
            {
                _logger.LogInformation($"+ Iteration: {idx}");
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

                _logger.LogInformation($"+ Waiting {delayInMilliseconds}ms");

                await Task.Delay(delayInMilliseconds);
            }

            _logger.LogInformation("Finished iterative change");
        }
    }
}