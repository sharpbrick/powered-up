using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public abstract class TachoMotor : BasicMotor
    {
        public byte ModeIndexSpeed { get; protected set; } = 1;
        public byte ModeIndexPosition { get; protected set; } = 2;

        public sbyte Speed { get; private set; }
        public int Position { get; private set; }

        public TachoMotor()
        { }
        protected TachoMotor(PoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        protected override bool OnPortValueChange(PortValueData data)
            => data.ModeIndex switch
            {
                var mode when (mode == ModeIndexSpeed) => OnSpeedChange(data as PortValueData<sbyte>),
                var mode when (mode == ModeIndexPosition) => OnPositionChange(data as PortValueData<int>),
                _ => base.OnPortValueChange(data),
            };

        private bool OnSpeedChange(PortValueData<sbyte> data)
        {
            Speed = data.InputValues[0];

            return true;
        }
        private bool OnPositionChange(PortValueData<int> data)
        {
            Position = data.InputValues[0];

            return true;
        }

        public async Task SetAccelerationTimeAsync(ushort timeInMs, PortOutputCommandSpeedProfile profileNumber = PortOutputCommandSpeedProfile.AccelerationProfile)
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

        public async Task SetDeccelerationTimeAsync(ushort timeInMs, PortOutputCommandSpeedProfile profileNumber = PortOutputCommandSpeedProfile.DeccelerationProfile)
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

        public async Task StartSpeedAsync(sbyte speed, byte maxPower, PortOutputCommandSpeedProfile profile)
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

        public async Task StartSpeedForTimeAsync(ushort time, sbyte speed, byte maxPower, PortOutputCommandSpecialSpeed endState, PortOutputCommandSpeedProfile profile)
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

        public async Task StartSpeedForDegreesAsync(uint degrees, sbyte speed, byte maxPower, PortOutputCommandSpecialSpeed endState, PortOutputCommandSpeedProfile profile)
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
    }
}