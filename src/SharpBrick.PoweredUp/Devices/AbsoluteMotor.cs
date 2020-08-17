using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class AbsoluteMotor : TachoMotor
    {
        protected SingleValueMode<short> _absoluteMode;
        public byte ModeIndexAbsolutePosition { get; protected set; } = 3;

        public short AbsolutePosition => _absoluteMode.SI;
        public short AbsolutePositionPct => _absoluteMode.Pct;
        public IObservable<Value<short>> AbsolutePositionObservable => _absoluteMode.Observable;

        public AbsoluteMotor()
        { }

        protected AbsoluteMotor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _absoluteMode = SingleValueMode<short>(ModeIndexAbsolutePosition);

            ObserveForPropertyChanged(_absoluteMode.Observable, nameof(AbsolutePosition), nameof(AbsolutePositionPct));
        }

        /// <summary>
        /// Start the motor with a speed of Speed using a maximum power of MaxPower and GOTO the Absolute position AbsPos. After position is reached the motor is stopped using the EndState mode of operation.
        /// 
        /// TachoMotor.StartSpeedForDegreesAsync support moving the motor by degrees. It operates relative of its current position.
        /// AbsoluteMotor.GotoAbsolutePositionAsync allows moving the motor to an absolute position. It operates relative to a changeable zero position.
        /// The Position(Observable) (POS) is reflecting the position relative to a changeable zero position (firmware in-memory encoded)
        /// The AbsolutePosition(Observable) (APOS) is reflecting the position relative to marked zero position (physically encoded).
        /// Position is resetable with SetZeroAsync, AbsolutePosition not.
        /// </summary>
        /// <param name="absolutePosition">Absolute Position (0 is the position at the start of the motor)</param>
        /// <param name="speed">The speed used to move to the absolute position.</param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> GotoPositionAsync(int absolutePosition, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            AssertValidSpeed(speed, nameof(speed));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandGotoAbsolutePositionMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                AbsolutePosition = absolutePosition,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Start the motors with individual speeds calculated from the common given Speed using a powerlevel limited to the MaxPower value. And the GOTO the Absolute positions: AbsPos1 and AbsPos2.
        /// 
        /// TachoMotor.StartSpeedForDegreesAsync support moving the motor by degrees. It operates relative of its current position.
        /// AbsoluteMotor.GotoAbsolutePositionAsync allows moving the motor to an absolute position. It operates relative to a changeable zero position.
        /// The Position(Observable) (POS) is reflecting the position relative to a changeable zero position (firmware in-memory encoded)
        /// The AbsolutePosition(Observable) (APOS) is reflecting the position relative to marked zero position (physically encoded).
        /// </summary>
        /// <param name="absolutePosition1">Absolute Position of motor 1 (0 is the position at the start of the motor)</param>
        /// <param name="absolutePosition2">Absolute Position of motor 2 (0 is the position at the start of the motor)</param>
        /// <param name="speed">The speed used to move to the absolute position.</param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> GotoPositionAsync(int absolutePosition1, int absolutePosition2, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            AssertValidSpeed(speed, nameof(speed));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();
            AssertIsVirtualPort();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandGotoAbsolutePosition2Message()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                AbsolutePosition1 = absolutePosition1,
                AbsolutePosition2 = absolutePosition2,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });

            return response;
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

            sbyte speed = 5;

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