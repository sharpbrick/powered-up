using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.TestScript
{
    public class TestScriptExecutionContext
    {
        public TestScriptExecutionContext(ILogger log)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ILogger Log { get; }
        public Hub CurrentHub { get; set; }

        public Task<bool> ConfirmAsync(string question)
        {
            Console.Write(question + " (Y/n)");

            var confirm = Console.ReadLine().Trim().ToUpperInvariant() != "N";

            if (confirm)
            {
                Log.LogInformation($"[OK] by User: {question}");
            }
            else
            {
                Log.LogError($"[FAIL] by User {question}");
            }

            return Task.FromResult(confirm);
        }

        public void Assert(int value, int min, int max)
        {
            if (value >= min && value <= max)
            {
                Log.LogInformation($"[OK] by Assert within expectations: {min} <= {value} <= {max}");
            }
            else
            {
                Log.LogError($"[FAIL] by Assert outside expectations: {min} <= {value} <= {max}");
            }
        }
    }
}
