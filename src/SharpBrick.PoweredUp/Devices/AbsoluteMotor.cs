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

        public async Task GotoAbsolutePositionAsync(int absolutePosition, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
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

        public async Task GotoAbsolutePositionAsync(int absolutePosition1, int absolutePosition2, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandGotoAbsolutePosition2Message()
            {
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
        }
    }
}