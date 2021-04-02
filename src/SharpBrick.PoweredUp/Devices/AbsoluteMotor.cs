using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class AbsoluteMotor : TachoMotor
    {
        protected SingleValueMode<short, short> _absoluteMode;
        public byte ModeIndexAbsolutePosition { get; protected set; } = 3;

        public short AbsolutePosition => _absoluteMode.SI;
        public short AbsolutePositionPct => _absoluteMode.Pct;
        public IObservable<Value<short, short>> AbsolutePositionObservable => _absoluteMode.Observable;

        public AbsoluteMotor()
        { }

        protected AbsoluteMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _absoluteMode = SingleValueMode<short, short>(ModeIndexAbsolutePosition);

            ObserveForPropertyChanged(_absoluteMode.Observable, nameof(AbsolutePosition), nameof(AbsolutePositionPct));
        }

        private async Task<int> GetPositionAsync()
        {
            AssertIsConnected();

            var awaitable = AbsolutePositionObservable.FirstAsync().GetAwaiter();

            await SetupNotificationAsync(ModeIndexAbsolutePosition, true);

            var result = await awaitable;

            await SetupNotificationAsync(ModeIndexAbsolutePosition, false);

            return result.SI;
        }

        /// <summary>
        /// Aligns the current position with the nearest absolute position 0.
        /// </summary>
        /// <returns></returns>
        public async Task GotoRealZeroAsync()
        {
            AssertIsConnected();

            var currentPosition = await GetPositionAsync();

            sbyte speed = 10;

            uint degrees;

            if (currentPosition < 0)
            {
                degrees = (uint)(-1 * currentPosition); // make position absolute since speed for degress only take positive degrees.
            }
            else
            {
                degrees = (uint)currentPosition;
                speed *= -1; // reverse direction if positive position
            }

            await StartSpeedForDegreesAsync(degrees, speed, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        }
    }
}