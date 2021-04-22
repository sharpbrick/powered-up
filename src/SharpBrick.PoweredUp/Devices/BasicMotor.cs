using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class BasicMotor : Device
    {
        protected SingleValueMode<sbyte, sbyte> _powerMode;
        public byte ModeIndexPower { get; protected set; } = 0;

        public sbyte Power => _powerMode.SI;
        public sbyte PowerPct => _powerMode.Pct;
        public IObservable<Value<sbyte, sbyte>> PowerObservable => _powerMode.Observable;

        public BasicMotor()
        { }

        public BasicMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _powerMode = SingleValueMode<sbyte, sbyte>(ModeIndexPower);

            ObserveForPropertyChanged(_powerMode.Observable, nameof(Power), nameof(PowerPct));
        }

        /// <summary>
        /// Starts the motor with full speed at the given power level.
        /// </summary>
        /// <param name="power">
        /// - Power levels in percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (floating): 0 
        /// - Stop Motor (breaking): 127
        /// </param>
        /// <returns>An awaitable Task.</returns>
        public async Task<PortFeedback> StartPowerAsync(sbyte power)
        {
            AssertValidPower(power, nameof(power));
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartPowerMessage(
                _portId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                power
            )
            {
                HubId = _hubId,
            });

            return response;
        }

        /// <summary>
        /// Stops the motor (brake; no movement afterwards)
        /// </summary>
        /// <returns></returns>
        public Task StopByBrakeAsync()
            => StartPowerAsync((sbyte)SpecialSpeed.Brake);

        /// <summary>
        /// Stops the motor (float; freely floating by inertia)
        /// </summary>
        /// <returns></returns>
        public Task StopByFloatAsync()
            => StartPowerAsync((sbyte)SpecialSpeed.Float);

        /// <summary>
        /// Start the pair on motors with full speed on given power values.
        /// </summary>
        /// <param name="powerOnMotor1">
        /// - Power levels in percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (floating): 0 
        /// - Stop Motor (breaking): 127
        /// </param>
        /// <param name="powerOnMotor2">
        /// - Power levels in percentage: 1 - 100 (CW), -1 - -100 (CCW)
        /// - Stop Motor (floating): 0 
        /// - Stop Motor (breaking): 127
        /// </param>
        /// <returns></returns>
        public async Task<PortFeedback> StartPowerAsync(sbyte powerOnMotor1, sbyte powerOnMotor2)
        {
            AssertValidPower(powerOnMotor1, nameof(powerOnMotor1));
            AssertValidPower(powerOnMotor2, nameof(powerOnMotor2));
            AssertIsConnected();
            AssertIsVirtualPort();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandStartPower2Message(
                _portId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                powerOnMotor1, powerOnMotor2
            )
            {
                HubId = _hubId,
            });

            return response;
        }

        protected void AssertValidPower(sbyte power, string argumentName)
        {
            if (
                  power < -100 ||
                  (power > 100 && power != 127)
              )
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }
    }
}