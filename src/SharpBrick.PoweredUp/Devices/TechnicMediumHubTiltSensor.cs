using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class TechnicMediumHubTiltSensor : Device, IPoweredUpDevice
    {
        protected MultiValueMode<short, short> _positionMode;
        protected SingleValueMode<int, int> _impactsMode;
        public byte ModeIndexPosition { get; protected set; } = 0;
        public byte ModeIndexImpacts { get; protected set; } = 1;
        public byte ModeIndexConfig { get; protected set; } = 2;

        public (short x, short y, short z) Position => (_positionMode.SI[0], _positionMode.SI[1], _positionMode.SI[2]);
        public int Impacts => _impactsMode.SI;
        public IObservable<(short x, short y, short z)> PositionObservable => _positionMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2]));
        public IObservable<Value<int, int>> ImpactsObservable => _impactsMode.Observable;

        public TechnicMediumHubTiltSensor()
        { }

        public TechnicMediumHubTiltSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _positionMode = MultiValueMode<short, short>(ModeIndexPosition);
            _impactsMode = SingleValueMode<int, int>(ModeIndexImpacts);

            ObserveForPropertyChanged(_positionMode.Observable, nameof(Position));
            ObserveForPropertyChanged(_impactsMode.Observable, nameof(Impacts));
        }

        /// <summary>
        /// Set the Tilt into ImpactCount mode and change (preset) the value to the given PresetValue.
        /// </summary>
        /// <param name="presetValue">Value between 0 and int.MaxValue</param>
        /// <returns></returns>
        public async Task<PortFeedback> TiltImpactPresetAsync(int presetValue)
        {
            AssertIsConnected();

            if (presetValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(presetValue), "PresetValue has to be between 0 and int.MaxValue");
            }

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandTiltImpactPresetMessage(
                _portId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                presetValue
            )
            {
                HubId = _hubId,
                ModeIndex = ModeIndexImpacts,
            });

            return response;
        }

        /// <summary>
        /// Setup Tilt ImpactThreshold and BumpHoldoff
        /// </summary>
        /// <param name="impactThreshold">Impact Threshold between 0 and 127.</param>
        /// <param name="bumpHoldoffInMs">Bump Holdoff between 10ms and 1270ms.</param>
        /// <returns></returns>
        public async Task<PortFeedback> TiltConfigImpactAsync(sbyte impactThreshold, short bumpHoldoffInMs)
        {
            AssertIsConnected();

            if (impactThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(impactThreshold), "Impact Threshold has to be between 0 and 127");
            }

            if (bumpHoldoffInMs < 10 || bumpHoldoffInMs > 1270)
            {
                throw new ArgumentOutOfRangeException(nameof(bumpHoldoffInMs), "Hold off has to be between 10 and 1270 ms (in steps of 10ms)");
            }

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandTiltConfigImpactMessage(
                _portId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                impactThreshold,
                (sbyte)((float)bumpHoldoffInMs / 10)
            )
            {
                HubId = _hubId,
                ModeIndex = ModeIndexConfig,
            });

            return response;
        }

        /// <summary>
        /// Set the Tilt into Orientation mode and set the Orientation value to Orientation.
        ///  
        /// DOES NOT work on technic medium hub (with or without feedback, with or without NoAction/CommandFeedback) (send e.g. as 08-00-81-63-11-51-02-01). 
        /// </summary>
        /// <param name="orientation">orientation of the tilt for 0 values.</param>
        /// <returns></returns>
        public async Task<PortFeedback> TiltConfigOrientationAsync(TiltConfigOrientation orientation)
        {
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandTiltConfigOrientationMessage(
                _portId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                orientation)
            {
                HubId = _hubId,
                ModeIndex = ModeIndexConfig, // TODO ???
            });

            return response;
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => (softwareVersion.ToString(), hardwareVersion.ToString()) switch
            {
                ("0.0.0.1", "0.0.0.1") =>
@"
0B-00-43-63-01-03-03-03-00-04-00
05-00-43-63-02
11-00-44-63-00-00-50-4F-53-00-00-00-00-00-00-00-00
0E-00-44-63-00-01-00-00-34-C3-00-00-34-43
0E-00-44-63-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-63-00-03-00-00-34-C3-00-00-34-43
0A-00-44-63-00-04-44-45-47-00
08-00-44-63-00-05-50-00
0A-00-44-63-00-80-03-01-03-00
11-00-44-63-01-00-49-4D-50-00-00-00-00-00-00-00-00
0E-00-44-63-01-01-00-00-00-00-00-00-C8-42
0E-00-44-63-01-02-00-00-00-00-00-00-C8-42
0E-00-44-63-01-03-00-00-00-00-00-00-C8-42
0A-00-44-63-01-04-43-4E-54-00
08-00-44-63-01-05-08-00
0A-00-44-63-01-80-01-02-03-00
11-00-44-63-02-00-43-46-47-00-00-00-00-00-00-00-00
0E-00-44-63-02-01-00-00-00-00-00-00-7F-43
0E-00-44-63-02-02-00-00-00-00-00-00-C8-42
0E-00-44-63-02-03-00-00-00-00-00-00-7F-43
0A-00-44-63-02-04-00-00-00-00
08-00-44-63-02-05-00-10
0A-00-44-63-02-80-02-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s)),
                _ => throw new NotSupportedException("SharpBrick.PoweredUp currently does not support this version of the sensor."),
            };
    }
}