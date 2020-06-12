using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Devices
{
    public class RgbLight : Device, IPowerdUpDevice
    {
        public RgbLight()
        { }

        public RgbLight(PoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public async Task SetRgbColorNoAsync(PortOutputCommandColors color)
        {
            await _protocol.SendMessageAsync(new PortOutputCommandSetRgbColorNoMessage()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                ColorNo = color,
            });
        }
        public async Task SetRgbColorsAsync(byte red, byte green, byte blue)
        {
            await _protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                PortId = _portId,
                Mode = 0x01,
                DeltaInterval = 10000,
                NotificationEnabled = false,
            });
            await _protocol.SendMessageAsync(new PortOutputCommandSetRgbColorNo2Message()
            {
                PortId = _portId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                RedColor = red,
                GreenColor = green,
                BlueColor = blue,
            });
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version sw, Version hw)
            => @"
0B-00-43-32-01-01-02-00-00-03-00
05-00-43-32-02
11-00-44-32-00-00-43-4F-4C-20-4F-00-00-00-00-00-00
0E-00-44-32-00-01-00-00-00-00-00-00-20-41
0E-00-44-32-00-02-00-00-00-00-00-00-C8-42
0E-00-44-32-00-03-00-00-00-00-00-00-20-41
0A-00-44-32-00-04-00-00-00-00
08-00-44-32-00-05-00-44
0A-00-44-32-00-80-01-00-01-00
11-00-44-32-01-00-52-47-42-20-4F-00-00-00-00-00-00
0E-00-44-32-01-01-00-00-00-00-00-00-7F-43
0E-00-44-32-01-02-00-00-00-00-00-00-C8-42
0E-00-44-32-01-03-00-00-00-00-00-00-7F-43
0A-00-44-32-01-04-00-00-00-00
08-00-44-32-01-05-00-10
0A-00-44-32-01-80-03-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));

    }
}