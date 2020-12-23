using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.Functions
{
    public class LinearSpeedChange : IterativeChange<sbyte>
    {
        private readonly ILogger<LinearSpeedChange> _logger;
        private TachoMotor _motor;
        private sbyte _startSpeed;

        public byte MaxPower { get; set; } = 100;

        public sbyte IterationStep { get; private set; }

        public LinearSpeedChange(ILogger<LinearSpeedChange> logger)
            : base(logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(TachoMotor motor, sbyte startSpeed, sbyte endSpeed, int steps, int milliseconds, CancellationTokenSource cts = default)
        {
            _logger.LogInformation($"Start execute {nameof(LinearSpeedChange)} ({startSpeed} - {endSpeed} over {steps} steps and {milliseconds}ms)");
            _startSpeed = startSpeed;
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));

            IterationStep = (sbyte)((endSpeed - startSpeed) / steps);
            _logger.LogInformation($"+ IterationStep: {IterationStep}");

            await IterativeExecuteAsync(steps, milliseconds / steps, cts);

            _logger.LogInformation($"Finished {nameof(LinearSpeedChange)} ");
        }

        protected override sbyte Function(int idx)
            => (sbyte)(_startSpeed + IterationStep * (idx + 1));

        protected override Task ChangeAsync(sbyte value, CancellationTokenSource cts)
            => _motor.StartSpeedAsync(value, MaxPower);
    }
}