using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public abstract class BasicMotor
    {
        protected readonly PoweredUpProtocol _protocol;
        protected readonly byte _portId;

        public BasicMotor()
        { }
        public BasicMotor(PoweredUpProtocol protocol, byte portId)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _portId = portId;
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

        public async Task SetAccelerationTime(ushort timeInMs, PortOutputCommandSpeedProfile profileNumber = PortOutputCommandSpeedProfile.AccelerationProfile)
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

        public async Task SetDeccelerationTime(ushort timeInMs, PortOutputCommandSpeedProfile profileNumber = PortOutputCommandSpeedProfile.DeccelerationProfile)
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
    }
}