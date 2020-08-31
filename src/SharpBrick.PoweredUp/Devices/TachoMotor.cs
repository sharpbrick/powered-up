using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class TachoMotor : BasicMotor
    {
        protected SingleValueMode<sbyte> _speedMode;
        protected SingleValueMode<int> _positionMode;
        public byte ModeIndexSpeed { get; protected set; } = 1;
        public byte ModeIndexPosition { get; protected set; } = 2;

        public sbyte Speed => _speedMode.SI;
        public sbyte SpeedPct => _speedMode.Pct;
        public IObservable<Value<sbyte>> SpeedObservable => _speedMode.Observable;

        public int Position => _positionMode.SI;
        public int PositionPct => _positionMode.Pct;
        public IObservable<Value<int>> PositionObservable => _positionMode.Observable;

        public TachoMotor()
        { }

        protected TachoMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _speedMode = SingleValueMode<sbyte>(ModeIndexSpeed);
            _positionMode = SingleValueMode<int>(ModeIndexPosition);

            ObserveForPropertyChanged(_speedMode.Observable, nameof(Speed), nameof(SpeedPct));
            ObserveForPropertyChanged(_positionMode.Observable, nameof(Position), nameof(PositionPct));
        }

        /// <summary>
        /// Sets an acceleration profile for the motor(s)
        /// </summary>
        /// <param name="timeInMs">
        /// Time in milliseconds of the acceleration (0 - 10000).
        /// 
        /// The Time is the duration time for an acceleration from 0 to 100%. I.e. a Time set to 1000 ms. should give an acceleration time of 300 ms. from 40% to 70%.
        /// 
        /// The selected profile will be used in all motor based commands if the profile is selected. The Time and Profile Number set for the profile is ignored if the profile is not selected.
        /// </param>
        /// <param name="profileNumber">Specify the acceleration profile used. Defaults to standard AccelerationProfile.</param>
        /// <returns></returns>
        public async Task<PortFeedback> SetAccelerationTimeAsync(ushort timeInMs, SpeedProfiles profileNumber = SpeedProfiles.AccelerationProfile)
        {
            if (timeInMs < 0 || timeInMs > 10_000)
            {
                throw new ArgumentOutOfRangeException(nameof(timeInMs));
            }
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandSetAccTimeMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = timeInMs,
                Profile = profileNumber,
            });

            return response;
        }

        /// <summary>
        /// Sets a deceleration profile for the motor(s)
        /// </summary>
        /// <param name="timeInMs">
        /// Time in milliseconds of the acceleration (0 - 10000).
        /// 
        /// The Time is the duration time for an deceleration from 100% to 0. I.e. a Time set to 1000 ms. should give an deceleration time of 400 ms. from 80% down to 40%
        /// The selected profile will be used in all motor based commands if the profile is selected. The Time and Profile Number set for the profile is ignored if the profile is not selected.
        /// </param>
        /// <param name="profileNumber">Specify the deceleration profile used. Defaults to standard DecelerationProfile.</param>
        /// <returns></returns>
        public async Task<PortFeedback> SetDecelerationTimeAsync(ushort timeInMs, SpeedProfiles profileNumber = SpeedProfiles.DecelerationProfile)
        {
            if (timeInMs < 0 || timeInMs > 10_000)
            {
                throw new ArgumentOutOfRangeException(nameof(timeInMs));
            }
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandSetDecTimeMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = timeInMs,
                Profile = profileNumber,
            });

            return response;
        }

        /// <summary>
        /// Start or Hold the motor(s) and keeping the speed/position not using power-levels greater than maxPower.
        /// </summary>
        /// <param name="speed">
        /// - Speed Level in Percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (hold position): 0 (use StartPower for floating and braking)
        /// </param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> StartSpeedAsync(sbyte speed, byte maxPower, SpeedProfiles profile)
        {
            AssertValidSpeed(speed, nameof(speed));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartSpeedMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Speed = speed,
                MaxPower = maxPower,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Start or Hold the motor(s) and keeping the speed/position not using power-levels greater than maxPower.
        /// </summary>
        /// <param name="speedOnMotor1">
        /// - Speed Level in Percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (hold position): 0 (use StartPower for floating and braking)
        /// </param>
        /// <param name="speedOnMotor2">
        /// - Speed Level in Percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (hold position): 0 (use StartPower for floating and braking)
        /// </param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> StartSpeedAsync(sbyte speedOnMotor1, sbyte speedOnMotor2, byte maxPower, SpeedProfiles profile)
        {
            AssertValidSpeed(speedOnMotor1, nameof(speedOnMotor1));
            AssertValidSpeed(speedOnMotor2, nameof(speedOnMotor2));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();
            AssertIsVirtualPort();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartSpeed2Message()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Speed1 = speedOnMotor1,
                Speed2 = speedOnMotor2,
                MaxPower = maxPower,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Start the motor(s) for Time ms. keeping a speed of Speed using a maximum power of MaxPower. After Time stopping the output using the EndState.
        /// </summary>
        /// <param name="timeInMs"></param>
        /// <param name="speed">
        /// - Speed Level in Percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (hold position): 0 (use StartPower for floating and braking)
        /// </param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> StartSpeedForTimeAsync(ushort timeInMs, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            AssertValidSpeed(speed, nameof(speed));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartSpeedForTimeMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = timeInMs,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Start the motors for Time ms. And try to keep individual speeds using Speed(X) while using a maximum power of MaxPower. After Time stopping the outputs using the EndState.
        /// </summary>
        /// <param name="timeInMs"></param>
        /// <param name="speedOnMotor1">
        /// - Speed Level in Percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (hold position): 0 (use StartPower for floating and braking)
        /// </param>
        /// <param name="speedOnMotor2">
        /// - Speed Level in Percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (hold position): 0 (use StartPower for floating and braking)
        /// </param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> StartSpeedForTimeAsync(ushort timeInMs, sbyte speedOnMotor1, sbyte speedOnMotor2, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            AssertValidSpeed(speedOnMotor1, nameof(speedOnMotor1));
            AssertValidSpeed(speedOnMotor2, nameof(speedOnMotor2));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();
            AssertIsVirtualPort();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartSpeedForTime2Message()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = timeInMs,
                Speed1 = speedOnMotor1,
                Speed2 = speedOnMotor2,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Start the motors for Degrees and try to keep individual speeds using Speed(X) while using a maximum power of MaxPower. After Degrees is reached stop the outputs using the EndState.
        /// 
        /// TachoMotor.StartSpeedForDegreesAsync support moving the motor by degrees. It operates relative of its current position.
        /// AbsoluteMotor.GotoAbsolutePositionAsync allows moving the motor to an absolute position. It operates relative to a changeable zero position.
        /// The Position(Observable) (POS) is reflecting the position relative to a changeable zero position (firmware in-memory encoded)
        /// The AbsolutePosition(Observable) (APOS) is reflecting the position relative to marked zero position (physically encoded).
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="speed"></param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> StartSpeedForDegreesAsync(uint degrees, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            AssertValidDegrees(degrees, nameof(degrees));
            AssertValidSpeed(speed, nameof(speed));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartSpeedForDegreesMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Degrees = degrees,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Start the motors for Degrees and try to keep individual speeds using Speed(X) while using a maximum power of MaxPower. After Degrees is reached stop the outputs using the EndState.
        /// 
        /// TachoMotor.StartSpeedForDegreesAsync support moving the motor by degrees. It operates relative of its current position.
        /// AbsoluteMotor.GotoAbsolutePositionAsync allows moving the motor to an absolute position. It operates relative to a changeable zero position.
        /// The Position(Observable) (POS) is reflecting the position relative to a changeable zero position (firmware in-memory encoded)
        /// The AbsolutePosition(Observable) (APOS) is reflecting the position relative to marked zero position (physically encoded).
        /// </summary>
        /// <param name="degrees">The degrees each motor should run. However, the combined degrees (2 * degrees) are split propertional among the speeds.</param>
        /// <param name="speedOnMotor1"></param>
        /// <param name="speedOnMotor2"></param>
        /// <param name="maxPower">Maximum Power level used.</param>
        /// <param name="endState">After time has expired, either Float, Hold or Brake.</param>
        /// <param name="profile">The speed profiles used (as flags) for acceleration and deceleration</param>
        /// <returns></returns>
        public async Task<PortFeedback> StartSpeedForDegreesAsync(uint degrees, sbyte speedOnMotor1, sbyte speedOnMotor2, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            AssertValidDegrees(degrees, nameof(degrees));
            AssertValidSpeed(speedOnMotor1, nameof(speedOnMotor1));
            AssertValidSpeed(speedOnMotor2, nameof(speedOnMotor2));
            AssertValidMaxPower(maxPower, nameof(maxPower));
            AssertIsConnected();
            AssertIsVirtualPort();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartSpeedForDegrees2Message()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Degrees = degrees,
                Speed1 = speedOnMotor1,
                Speed2 = speedOnMotor2,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });

            return response;
        }

        /// <summary>
        /// Reset the position encoder in the motor
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task<PortFeedback> SetZeroAsync(int position = 0)
        {
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandPresetEncoderMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                ModeIndex = ModeIndexPosition, // GotoAbsolutePosition and input POS is aligned with this adjustment
                Position = position,
            });

            return response;
        }

        protected void AssertValidSpeed(sbyte speed, string argumentName)
        {
            if (speed < -100 || speed > 100)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        protected void AssertValidMaxPower(byte power, string argumentName)
        {
            if (power > 100)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        protected void AssertValidDegrees(uint degrees, string argumentName)
        {
            if (degrees > 10_000_000)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
    }
}