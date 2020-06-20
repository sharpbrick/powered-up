using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class TachoMotor : BasicMotor
    {
        public byte ModeIndexSpeed { get; protected set; } = 1;
        public byte ModeIndexPosition { get; protected set; } = 2;

        public sbyte Speed { get; private set; } = 0;
        public sbyte SpeedPct { get; private set; } = 0;
        public IObservable<Value<sbyte>> SpeedObservable { get; }

        public int Position { get; private set; } = 0;
        public int PositionPct { get; private set; } = 0;
        public IObservable<Value<int>> PositionObservable { get; }

        public TachoMotor()
        { }

        protected TachoMotor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {

            SpeedObservable = CreateSinglePortModeValueObservable<sbyte>(ModeIndexSpeed);
            PositionObservable = CreateSinglePortModeValueObservable<int>(ModeIndexPosition);

            ObserveOnLocalProperty(SpeedObservable, v => Speed = v.SI, v => SpeedPct = v.Pct);
            ObserveOnLocalProperty(PositionObservable, v => Position = v.SI, v => PositionPct = v.Pct);
        }

        public async Task SetAccelerationTimeAsync(ushort timeInMs, SpeedProfiles profileNumber = SpeedProfiles.AccelerationProfile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandSetAccTimeMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = timeInMs,
                Profile = profileNumber,
            });
        }

        public async Task SetDeccelerationTimeAsync(ushort timeInMs, SpeedProfiles profileNumber = SpeedProfiles.DeccelerationProfile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandSetDecTimeMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = timeInMs,
                Profile = profileNumber,
            });
        }

        public async Task StartSpeedAsync(sbyte speed, byte maxPower, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartSpeedMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Speed = speed,
                MaxPower = maxPower,
                Profile = profile,
            });
        }

        public async Task StartSpeedAsync(sbyte speed1, sbyte speed2, byte maxPower, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartSpeed2Message()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Speed1 = speed1,
                Speed2 = speed2,
                MaxPower = maxPower,
                Profile = profile,
            });
        }

        public async Task StartSpeedForTimeAsync(ushort time, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartSpeedForTimeMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = time,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });
        }

        public async Task StartSpeedForTimeAsync(ushort time, sbyte speed1, sbyte speed2, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartSpeedForTime2Message()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Time = time,
                Speed1 = speed1,
                Speed2 = speed2,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });
        }

        public async Task StartSpeedForDegreesAsync(uint degrees, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartSpeedForDegreesMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Degrees = degrees,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });
        }


        public async Task StartSpeedForDegreesAsync(uint degrees, sbyte speed1, sbyte speed2, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartSpeedForDegrees2Message()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Degrees = degrees,
                Speed1 = speed1,
                Speed2 = speed2,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });
        }
    }
}