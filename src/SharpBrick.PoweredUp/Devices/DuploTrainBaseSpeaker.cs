using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{

    public class DuploTrainBaseSpeaker : Device, IPoweredUpDevice
    {
        protected SingleValueMode<sbyte, sbyte> _soundMode;

        public byte ModeIndexTone { get; protected set; } = 0;
        public byte ModeIndexSound { get; protected set; } = 1;
        public byte ModeIndexUiSound { get; protected set; } = 2;

        public DuploTrainBaseSpeaker()
        { }

        public DuploTrainBaseSpeaker(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _soundMode = SingleValueMode<sbyte, sbyte>(ModeIndexSound);
        }

        public async Task<PortFeedback> PlaySoundAsync(DuploTrainBaseSound sound)
        {
            await this.SetupNotificationAsync(ModeIndexSound, false);
            var response = await _soundMode.WriteDirectModeDataAsync((byte)sound);

            return response;
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-01-01-01-03-00-00-07-00
05-00-43-01-02
11-00-44-01-00-00-54-4F-4E-45-00-00-00-00-00-00-00
0E-00-44-01-00-01-00-00-00-00-00-00-20-41
0E-00-44-01-00-02-00-00-00-00-00-00-C8-42
0E-00-44-01-00-03-00-00-00-00-00-00-20-41
0A-00-44-01-00-04-69-64-78-00
08-00-44-01-00-05-00-04
0A-00-44-01-00-80-01-00-03-00
11-00-44-01-01-00-53-4F-55-4E-44-00-00-00-00-00-00
0E-00-44-01-01-01-00-00-00-00-00-00-20-41
0E-00-44-01-01-02-00-00-00-00-00-00-C8-42
0E-00-44-01-01-03-00-00-00-00-00-00-20-41
0A-00-44-01-01-04-69-64-78-00
08-00-44-01-01-05-00-44
0A-00-44-01-01-80-01-00-03-00
11-00-44-01-02-00-55-49-20-53-4E-44-00-00-00-00-00
0E-00-44-01-02-01-00-00-00-00-00-00-20-41
0E-00-44-01-02-02-00-00-00-00-00-00-C8-42
0E-00-44-01-02-03-00-00-00-00-00-00-20-41
0A-00-44-01-02-04-69-64-78-00
08-00-44-01-02-05-00-04
0A-00-44-01-02-80-01-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}