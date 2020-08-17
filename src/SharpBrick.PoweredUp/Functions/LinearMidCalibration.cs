using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static SharpBrick.PoweredUp.Directions;

namespace SharpBrick.PoweredUp.Functions
{
    public class LinearMidCalibration
    {
        private readonly ILogger<LinearMidCalibration> _logger;

        public byte MaxPower { get; set; } = 20;
        public byte Speed { get; set; } = 10;

        public int RangeResult { get; private set; }
        public uint ExtendResult { get; private set; }

        public LinearMidCalibration(ILogger<LinearMidCalibration> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(TachoMotor motor)
        {
            if (motor is null)
            {
                throw new ArgumentNullException(nameof(motor));
            }

            _logger.LogInformation("Start Linear Mid Calibration");
            await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexSpeed, motor.ModeIndexPosition);
            await motor.SetupNotificationAsync(motor.ModeIndexSpeed, true, 1);
            await motor.SetupNotificationAsync(motor.ModeIndexPosition, true, 1);
            await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

            _logger.LogInformation("Start CW");
            await motor.StartPowerAsync((sbyte)(CW * MaxPower));

            await motor.SpeedObservable.Where(x => x.SI == 0).FirstAsync().GetAwaiter();
            _logger.LogInformation("Reached End by detecting no speed movement.");

            var cwVal = motor.Position;
            _logger.LogInformation($"CW End at {cwVal}.");

            _logger.LogInformation("Start CCW");
            await motor.StartPowerAsync((sbyte)(CCW * MaxPower));

            await motor.SpeedObservable.Where(x => x.SI == 0).FirstAsync().GetAwaiter();
            _logger.LogInformation("Reached End by detecting no speed movement.");

            var ccwVal = motor.Position;
            _logger.LogInformation($"CW End at {ccwVal}.");

            await motor.StopByBrakeAsync();

            var range = Math.Abs(cwVal - ccwVal);
            var extend = (uint)(range / 2);
            _logger.LogInformation($"Total Range: {range} Extend from center: {extend}");

            await motor.StartSpeedForDegreesAsync(extend, (sbyte)(CW * Speed), MaxPower, SpecialSpeed.Hold, SpeedProfiles.None);

            await motor.SpeedObservable.Where(x => x.SI == 0).FirstAsync().GetAwaiter();
            await motor.SetZeroAsync();
            _logger.LogInformation($"Moved to center. Reset Position.");

            await motor.UnlockFromCombinedModeNotificationSetupAsync(false);

            RangeResult = range;
            ExtendResult = extend;
            _logger.LogInformation($"End Linear Mid Calibration");
        }
    }
}