using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class AbsoluteMotor : TachoMotor
    {
        public byte ModeIndexAbsolutePosition { get; protected set; } = 3;

        public short AbsolutePosition { get; private set; } = 0;
        public short AbsolutePositionPct { get; private set; } = 0;
        public IObservable<Value<short>> AbsolutePositionObservable { get; }

        public AbsoluteMotor()
        { }

        protected AbsoluteMotor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            AbsolutePositionObservable = CreateSinglePortModeValueObservable<short>(ModeIndexAbsolutePosition);

            ObserveOnLocalProperty(AbsolutePositionObservable, v => AbsolutePosition = v.SI, v => AbsolutePositionPct = v.Pct);
        }

        /// <summary>
        /// Start the motor with a speed of Speed using a maximum power of MaxPower and GOTO the Absolute position AbsPos. After position is reached the motor is stopped using the EndState mode of operation.
        /// </summary>
        /// <param name="absolutePosition">Absolute Position (0 is the position at the start of the motor)</param>
        /// <param name="speed">The speed used to move to the absolute position.</param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> GotoAbsolutePositionAsync(int absolutePosition, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
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
        /// </summary>
        /// <param name="absolutePosition1">Absolute Position of motor 1 (0 is the position at the start of the motor)</param>
        /// <param name="absolutePosition2">Absolute Position of motor 2 (0 is the position at the start of the motor)</param>
        /// <param name="speed">The speed used to move to the absolute position.</param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> GotoAbsolutePositionAsync(int absolutePosition1, int absolutePosition2, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
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
    }
}