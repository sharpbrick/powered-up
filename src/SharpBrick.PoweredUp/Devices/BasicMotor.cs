using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public abstract class BasicMotor : Device
    {
        public byte ModeIndexPower { get; protected set; } = 0;
        public sbyte Power { get; private set; } = 0;

        public BasicMotor()
        { }

        public BasicMotor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        protected override bool OnPortValueChange(PortValueData data)
            => data.ModeIndex switch
            {
                var mode when (mode == ModeIndexPower) => OnPowerChange(data as PortValueData<sbyte>),
                _ => base.OnPortValueChange(data),
            };

        private bool OnPowerChange(PortValueData<sbyte> data)
        {
            Power = data.InputValues[0];

            return true;
        }

        public async Task StartPowerAsync(sbyte power)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandStartPowerMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Power = power,
            });
        }
    }
}