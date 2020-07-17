using System;
using System.Globalization;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubAttachedIOEncoderTest
    {
        [Theory]
        [InlineData("0F-00-04-32-01-17-00-00-00-00-10-00-00-00-10", DeviceType.RgbLight, 50, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-3B-01-15-00-00-00-00-10-00-00-00-10", DeviceType.Current, 59, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-3C-01-14-00-00-00-00-10-00-00-00-10", DeviceType.Voltage, 60, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-3D-01-3C-00-00-00-00-10-00-00-00-10", DeviceType.TechnicMediumHubTemperatureSensor, 61, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-60-01-3C-00-01-00-00-00-01-00-00-00", DeviceType.TechnicMediumHubTemperatureSensor, 96, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-61-01-39-00-01-00-00-00-01-00-00-00", DeviceType.TechnicMediumHubAccelerometer, 97, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-62-01-3A-00-01-00-00-00-01-00-00-00", DeviceType.TechnicMediumHubGyroSensor, 98, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-63-01-3B-00-01-00-00-00-01-00-00-00", DeviceType.TechnicMediumHubTiltSensor, 99, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-64-01-36-00-01-00-00-00-01-00-00-00", DeviceType.TechnicMediumHubGestSensor, 100, "0.0.0.1", "0.0.0.1")]
        public void HubAttachedIOEncoder_Decode_Attached<T>(string messageAsString, DeviceType expectedType, byte expectedPortId, string expectedHwVersion, string expectedSwVersion)
        {
            // arrange
            var data = BytesStringUtil.StringToData(messageAsString).AsSpan().Slice(3);

            // act
            var message = new HubAttachedIOEncoder().Decode(0x00, data) as HubAttachedIOForAttachedDeviceMessage;

            // assert
            Assert.Equal(expectedPortId, message.PortId);
            Assert.Equal(expectedType, message.IOTypeId);
            Assert.Equal(new Version(expectedHwVersion), message.HardwareRevision);
            Assert.Equal(new Version(expectedSwVersion), message.SoftwareRevision);

            // reverse test
            var reverseMessage = new HubAttachedIOForAttachedDeviceMessage()
            {
                Event = HubAttachedIOEvent.AttachedIO,
                IOTypeId = expectedType,
                PortId = expectedPortId,
                HardwareRevision = Version.Parse(expectedHwVersion),
                SoftwareRevision = Version.Parse(expectedSwVersion),
            };

            // act
            var reverseData = MessageEncoder.Encode(reverseMessage, null);
            var reverseDataAsString = BytesStringUtil.DataToString(reverseData);

            Assert.Equal(messageAsString, reverseDataAsString);
        }


        [Theory]
        [InlineData("09-00-04-88-02-2E-00-01-02", DeviceType.TechnicLargeLinearMotor, 0x88, 0x01, 0x02)]
        public void HubAttachedIOEncoder_Decode_AttachedVirutalIO(string messageAsString, DeviceType expectedType, byte expectedPortId, byte portA, byte portB)
        {
            // arrange
            var data = BytesStringUtil.StringToData(messageAsString).AsSpan().Slice(3);

            // act
            var message = new HubAttachedIOEncoder().Decode(0x00, data) as HubAttachedIOForAttachedVirtualDeviceMessage;

            // assert
            Assert.Equal(expectedPortId, message.PortId);
            Assert.Equal(expectedType, message.IOTypeId);
            Assert.Equal(portA, message.PortAId);
            Assert.Equal(portB, message.PortBId);
        }
    }
}