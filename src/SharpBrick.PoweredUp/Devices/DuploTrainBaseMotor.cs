using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class DuploTrainBaseMotor : Device, IPoweredUpDevice
    {
        public SingleValueMode<int, int> _onSecMode;

        public byte ModeIndexMotor { get; protected set; } = 0;
        public byte ModeIndexOnSec { get; protected set; } = 1;

        public int OnSeconds => _onSecMode.SI;
        public IObservable<int> OnSecondsObservable => _onSecMode.Observable.Select(x => x.SI);

        public DuploTrainBaseMotor()
        { }

        public DuploTrainBaseMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _onSecMode = SingleValueMode<int, int>(ModeIndexOnSec);

            ObserveForPropertyChanged(_onSecMode.Observable, nameof(OnSeconds));
        }

        public void ExtendPortMode(PortModeInfo modeInfo)
        {
            if (modeInfo.ModeIndex == ModeIndexOnSec)
            {
                modeInfo.DisablePercentage = true;
            }
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

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-00-01-03-02-02-00-01-00
05-00-43-00-02
11-00-44-00-00-00-54-20-4D-4F-54-00-00-00-00-00-00
0E-00-44-00-00-01-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-03-00-00-C8-C2-00-00-C8-42
0A-00-44-00-00-04-70-77-72-00
08-00-44-00-00-05-00-50
0A-00-44-00-00-80-01-00-03-00
11-00-44-00-01-00-4F-4E-53-45-43-00-00-00-00-00-00
0E-00-44-00-01-01-00-00-00-00-00-00-80-3F
0E-00-44-00-01-02-00-00-00-00-00-00-C8-42
0E-00-44-00-01-03-00-00-00-00-00-00-80-3F
0A-00-44-00-01-04-73-65-63-00
08-00-44-00-01-05-08-00
0A-00-44-00-01-80-01-02-04-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}