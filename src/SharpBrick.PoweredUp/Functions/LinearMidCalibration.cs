using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static SharpBrick.PoweredUp.Directions;

namespace SharpBrick.PoweredUp.Functions
{
    public class LinearMidCalibration
    {
        private IServiceProvider _serviceProvider;

        public byte MaxPower { get; set; } = 10;
        public byte Speed { get; set; } = 10;

        public LinearMidCalibration(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task ExecuteAsync(TachoMotor motor)
        {
            if (motor is null)
            {
                throw new ArgumentNullException(nameof(motor));
            }

            var logger = _serviceProvider.GetService<ILoggerFactory>().CreateLogger<LinearMidCalibration>();

            logger.LogInformation("Start Linear Mid Calibration");
            await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexSpeed, motor.ModeIndexPosition);
            await motor.SetupNotificationAsync(motor.ModeIndexSpeed, true, 1);
            await motor.SetupNotificationAsync(motor.ModeIndexPosition, true, 1);
            await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

            logger.LogInformation("Start CW");
            await motor.StartPowerAsync((sbyte)(CW * MaxPower));

            await motor.SpeedObservable.Where(x => x.SI == 0).FirstAsync().GetAwaiter();
            logger.LogInformation("Reached End by detecting no speed movement.");

            var cwVal = motor.Position;
            logger.LogInformation($"CW End at {cwVal}.");

            logger.LogInformation("Start CCW");
            await motor.StartPowerAsync((sbyte)(CCW * MaxPower));

            await motor.SpeedObservable.Where(x => x.SI == 0).FirstAsync().GetAwaiter();
            logger.LogInformation("Reached End by detecting no speed movement.");

            var ccwVal = motor.Position;
            logger.LogInformation($"CW End at {ccwVal}.");

            await motor.StopByBrakeAsync();

            var range = Math.Abs(cwVal - ccwVal);
            var extend = (uint)(range / 2);
            logger.LogInformation($"Total Range: {range} Extend from center: {extend}");

            await motor.StartSpeedForDegreesAsync(extend, (sbyte)(CW * Speed), MaxPower, SpecialSpeed.Hold, SpeedProfiles.None);

            await motor.SpeedObservable.Where(x => x.SI == 0).FirstAsync().GetAwaiter();
            await motor.SetZeroAsync();
            logger.LogInformation($"Moved to center. Reset Position.");

            await motor.UnlockFromCombinedModeNotificationSetupAsync(false);
            logger.LogInformation($"End Linear Mid Calibration");
        }
    }
}