using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public abstract class AbsoluteMotor : TachoMotor
    {
        public byte ModeIndexAbsolutePosition { get; protected set; } = 3;
        public short AbsolutePosition { get; private set; } // ModeIndex = 3
        public AbsoluteMotor()
        { }
        protected AbsoluteMotor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        protected override bool OnPortValueChange(PortValueData data)
            => data.ModeIndex switch
            {
                var mode when (mode == ModeIndexAbsolutePosition) => OnAbsolutePositionChange(data as PortValueData<short>),
                _ => base.OnPortValueChange(data),
            };

        private bool OnAbsolutePositionChange(PortValueData<short> data)
        {
            AbsolutePosition = data.InputValues[0];

            return true;
        }

        public async Task GotoAbsolutePositionAsync(int absolutePosition, sbyte speed, byte maxPower, PortOutputCommandSpecialSpeed endState, PortOutputCommandSpeedProfile profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandGotoAbsolutePositionMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                AbsolutePosition = absolutePosition,
                Speed = speed,
                MaxPower = maxPower,
                EndState = endState,
                Profile = profile,
            });
        }
    }
}