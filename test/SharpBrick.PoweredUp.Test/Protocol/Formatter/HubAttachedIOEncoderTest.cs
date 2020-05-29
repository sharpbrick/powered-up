using System;
using System.Globalization;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubAttachedIOEncoderTest
    {
        [Theory]
        [InlineData("0F-00-04-32-01-17-00-00-00-00-10-00-00-00-10", HubAttachedIOType.RgbLight, 50, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-3B-01-15-00-00-00-00-10-00-00-00-10", HubAttachedIOType.Current, 59, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-3C-01-14-00-00-00-00-10-00-00-00-10", HubAttachedIOType.Voltage, 60, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-3D-01-3C-00-00-00-00-10-00-00-00-10", HubAttachedIOType.TechnicMediumHubTemperatureSensor, 61, "1.0.0.0", "1.0.0.0")]
        [InlineData("0F-00-04-60-01-3C-00-01-00-00-00-01-00-00-00", HubAttachedIOType.TechnicMediumHubTemperatureSensor, 96, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-61-01-39-00-01-00-00-00-01-00-00-00", HubAttachedIOType.TechnicMediumHubAccelerometer, 97, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-62-01-3A-00-01-00-00-00-01-00-00-00", HubAttachedIOType.TechnicMediumHubGyroSensor, 98, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-63-01-3B-00-01-00-00-00-01-00-00-00", HubAttachedIOType.TechnicMediumHubTiltSensor, 99, "0.0.0.1", "0.0.0.1")]
        [InlineData("0F-00-04-64-01-36-00-01-00-00-00-01-00-00-00", HubAttachedIOType.TechnicMediumHubGestSensor, 100, "0.0.0.1", "0.0.0.1")]
        public void HubAttachedIOEncoder_Decode_Attached<T>(string messageAsString, HubAttachedIOType expectedType, byte expectedPortId, string expectedHwVersion, string expectedSwVersion)
        {
            // arrange
            var data = StringToData(messageAsString).AsSpan().Slice(3);

            // act
            var message = new HubAttachedIOEncoder().Decode(data) as HubAttachedIOForAttachedDeviceMessage;

            // assert
            Assert.Equal(expectedPortId, message.PortId);
            Assert.Equal(expectedType, message.IOTypeId);
            Assert.Equal(new Version(expectedHwVersion), message.HardwareRevision);
            Assert.Equal(new Version(expectedSwVersion), message.SoftwareRevision);
        }

        private byte[] StringToData(string messageAsString)
            => messageAsString.Split("-").Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
    }
}