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
    public class MoveHubTiltSensor : Device, IPoweredUpDevice
    {
        protected MultiValueMode<sbyte, sbyte> _twoAxisFullMode;
        protected SingleValueMode<sbyte, sbyte> _twoAxisStateMode;
        protected SingleValueMode<sbyte, sbyte> _threeAxisStateMode;
        protected SingleValueMode<int, int> _impactsMode;
        protected MultiValueMode<sbyte, sbyte> _threeAxisFullMode;

        /// <summary>
        /// Two axis full values
        /// </summary>
        public byte ModeIndexTwoAxisFull { get; protected set; } = 0;
        /// <summary>
        /// Two Axis simple state of orientation
        /// </summary>
        public byte ModeIndexTwoAxisState { get; protected set; } = 1;
        /// <summary>
        /// Three Axis simple state of orientation which provides more states than <see cref="ModeIndexTwoAxisState"/>
        /// </summary>
        public byte ModeIndexThreeAxisState { get; protected set; } = 2;
        /// <summary>
        /// Count of impacts
        /// </summary>
        public byte ModeIndexImpacts { get; protected set; } = 3;
        /// <summary>
        /// Three axis full values
        /// </summary>
        public byte ModeIndexThreeAxisFull { get; protected set; } = 4;
        public byte ModeIndexOrientationConfig { get; protected set; } = 5;
        public byte ModeIndexImpactsConfig { get; protected set; } = 6;
        public byte ModeIndexCalibration { get; protected set; } = 7;

        public (sbyte roll, sbyte pitch) TwoAxisFull => (_twoAxisFullMode.SI[0], _twoAxisFullMode.SI[1]);
        public MoveHubTiltSimpleOrientation TwoAxisState => (MoveHubTiltSimpleOrientation)_twoAxisStateMode.SI;
        public MoveHubTiltOrientation ThreeAxisState => (MoveHubTiltOrientation)_threeAxisStateMode.SI;
        public int Impacts => _impactsMode.SI;
        public (sbyte roll, sbyte pitch, sbyte yaw) ThreeAxisFull => (_threeAxisFullMode.SI[0], _threeAxisFullMode.SI[1], _threeAxisFullMode.SI[2]);

        public IObservable<(sbyte roll, sbyte pitch)> TwoAxisFullObservable => _twoAxisFullMode.Observable.Select(v => (v.SI[0], v.SI[1]));
        public IObservable<MoveHubTiltSimpleOrientation> TwoAxisStateObservable => _twoAxisStateMode.Observable.Select(x => (MoveHubTiltSimpleOrientation)x.SI);
        public IObservable<MoveHubTiltOrientation> ThreeAxisStateObservable => _threeAxisStateMode.Observable.Select(x => (MoveHubTiltOrientation)x.SI);
        public IObservable<Value<int, int>> ImpactsObservable => _impactsMode.Observable;
        public IObservable<(sbyte roll, sbyte pitch, sbyte yaw)> ThreeAxisFullObservable => _threeAxisFullMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2]));

        public MoveHubTiltSensor()
        { }

        public MoveHubTiltSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _twoAxisFullMode = MultiValueMode<sbyte, sbyte>(ModeIndexTwoAxisFull);
            _twoAxisStateMode = SingleValueMode<sbyte, sbyte>(ModeIndexTwoAxisState);
            _threeAxisStateMode = SingleValueMode<sbyte, sbyte>(ModeIndexThreeAxisState);
            _impactsMode = SingleValueMode<int, int>(ModeIndexImpacts);
            _threeAxisFullMode = MultiValueMode<sbyte, sbyte>(ModeIndexThreeAxisFull);

            ObserveForPropertyChanged(_twoAxisFullMode.Observable, nameof(TwoAxisFull));
            ObserveForPropertyChanged(_twoAxisStateMode.Observable, nameof(TwoAxisState));
            ObserveForPropertyChanged(_threeAxisStateMode.Observable, nameof(ThreeAxisState));
            ObserveForPropertyChanged(_impactsMode.Observable, nameof(Impacts));
            ObserveForPropertyChanged(_threeAxisFullMode.Observable, nameof(ThreeAxisFull));
        }

        public void ExtendPortMode(PortModeInfo modeInfo)
        {
            // Percentage is disabled for three axis full (ACCEL) since in some cases the hub 
            // can report a larger than expected value which makes the percentage go beyond an sbyte min/max values and generate an exception
            // TODO: This can be removed when #126 is implemented
            if (modeInfo.ModeIndex == ModeIndexThreeAxisFull)
            {
                modeInfo.DisablePercentage = true;
            }
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
                ModeIndex = ModeIndexImpactsConfig,
            });

            return response;
        }

        /// <summary>
        /// Set the Tilt into Orientation mode and set the Orientation value to Orientation.
        /// </summary>
        /// <param name="orientation">orientation of the tilt for 0 values.</param>
        /// <returns></returns>
        public async Task<PortFeedback> TiltConfigOrientationAsync(TiltConfigOrientation orientation)
        {
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new PortOutputCommandTiltConfigOrientationMessage(
                _portId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                orientation
            )
            {
                HubId = _hubId,
                ModeIndex = ModeIndexOrientationConfig,
            });

            return response;
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            =>
@"
0B-00-43-3A-01-06-08-FF-00-00-00
07-00-43-3A-02-1F-00
11-00-44-3A-00-00-41-4E-47-4C-45-00-00-00-00-00-00
0E-00-44-3A-00-01-00-00-B4-C2-00-00-B4-42
0E-00-44-3A-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-3A-00-03-00-00-B4-C2-00-00-B4-42
0A-00-44-3A-00-04-44-45-47-00
08-00-44-3A-00-05-50-00
0A-00-44-3A-00-80-02-00-03-00
11-00-44-3A-01-00-54-49-4C-54-00-00-00-00-00-00-00
0E-00-44-3A-01-01-00-00-00-00-00-00-20-41
0E-00-44-3A-01-02-00-00-00-00-00-00-C8-42
0E-00-44-3A-01-03-00-00-00-00-00-00-20-41
0A-00-44-3A-01-04-44-49-52-00
08-00-44-3A-01-05-44-00
0A-00-44-3A-01-80-01-00-01-00
11-00-44-3A-02-00-4F-52-49-4E-54-00-00-00-00-00-00
0E-00-44-3A-02-01-00-00-00-00-00-00-A0-40
0E-00-44-3A-02-02-00-00-00-00-00-00-C8-42
0E-00-44-3A-02-03-00-00-00-00-00-00-A0-40
0A-00-44-3A-02-04-44-49-52-00
08-00-44-3A-02-05-10-00
0A-00-44-3A-02-80-01-00-01-00
11-00-44-3A-03-00-49-4D-50-43-54-00-00-00-00-00-00
0E-00-44-3A-03-01-00-00-00-00-00-00-C8-42
0E-00-44-3A-03-02-00-00-00-00-00-00-C8-42
0E-00-44-3A-03-03-00-00-00-00-00-00-C8-42
0A-00-44-3A-03-04-49-4D-50-00
08-00-44-3A-03-05-08-00
0A-00-44-3A-03-80-01-02-04-00
11-00-44-3A-04-00-41-43-43-45-4C-00-00-00-00-00-00
0E-00-44-3A-04-01-00-00-82-C2-00-00-82-42
0E-00-44-3A-04-02-00-00-C8-C2-00-00-C8-42
0E-00-44-3A-04-03-00-00-82-C2-00-00-82-42
0A-00-44-3A-04-04-41-43-43-00
08-00-44-3A-04-05-10-00
0A-00-44-3A-04-80-03-00-03-00
11-00-44-3A-05-00-4F-52-5F-43-46-00-00-00-00-00-00
0E-00-44-3A-05-01-00-00-00-00-00-00-C0-40
0E-00-44-3A-05-02-00-00-00-00-00-00-C8-42
0E-00-44-3A-05-03-00-00-00-00-00-00-C0-40
0A-00-44-3A-05-04-53-49-44-00
08-00-44-3A-05-05-10-00
0A-00-44-3A-05-80-01-00-01-00
11-00-44-3A-06-00-49-4D-5F-43-46-00-00-00-00-00-00
0E-00-44-3A-06-01-00-00-00-00-00-00-7F-43
0E-00-44-3A-06-02-00-00-00-00-00-00-C8-42
0E-00-44-3A-06-03-00-00-00-00-00-00-7F-43
0A-00-44-3A-06-04-53-45-4E-00
08-00-44-3A-06-05-10-00
0A-00-44-3A-06-80-02-00-03-00
11-00-44-3A-07-00-43-41-4C-49-42-00-00-00-00-00-00
0E-00-44-3A-07-01-00-00-00-00-00-00-7F-43
0E-00-44-3A-07-02-00-00-00-00-00-00-C8-42
0E-00-44-3A-07-03-00-00-00-00-00-00-7F-43
0A-00-44-3A-07-04-43-41-4C-00
08-00-44-3A-07-05-10-00
0A-00-44-3A-07-80-03-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}