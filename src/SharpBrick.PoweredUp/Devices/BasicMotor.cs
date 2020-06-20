using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class BasicMotor : Device
    {
        public byte ModeIndexPower { get; protected set; } = 0;

        public sbyte Power { get; private set; } = 0;
        public sbyte PowerPct { get; private set; } = 0;
        public IObservable<Value<sbyte>> PowerObservable { get; }

        public BasicMotor()
        { }

        public BasicMotor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            PowerObservable = CreateSinglePortModeValueObservable<sbyte>(ModeIndexPower);

            ObserveOnLocalProperty(PowerObservable, v => Power = v.SI, v => PowerPct = v.Pct);
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

        public Task StartPowerAsync(sbyte power1, sbyte power2)
            => _protocol.SendMessageAsync(new PortOutputCommandStartPower2Message()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Power1 = power1,
                Power2 = power2,
            });
    }
}