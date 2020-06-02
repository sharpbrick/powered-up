//0A-00-45-63-21-00-F9-FF-FD-FF

using System;
using System.Linq;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortValueSingleEncoderTest
    {
        [Theory]
        [InlineData("0A-00-45-63-21-00-F9-FF-FD-FF", 0x63, PortModeInformationDataType.Int16, new short[] { 33, -7, -3 })]
        public void PortValueSingleEncoder_Decode_Short(string dataAsString, byte expectedPortId, PortModeInformationDataType expectedDataType, short[] expectedData)
            => PortValueSingleEncoder_Decode<short>(dataAsString, expectedPortId, expectedDataType, expectedData);
        public void PortValueSingleEncoder_Decode<T>(string dataAsString, byte expectedPortId, PortModeInformationDataType expectedDataType, T[] expectedData)
        {
            var knowledge = new ProtocolKnowledge();

            KnowledgeManager.ApplyDynamicProtocolKnowledge(new HubAttachedIOForAttachedDeviceMessage() { HubId = 0, IOTypeId = HubAttachedIOType.TechnicMediumHubTiltSensor, MessageType = MessageType.HubAttachedIO, Event = HubAttachedIOEvent.AttachedIO, PortId = 0x63, HardwareRevision = new Version("0.0.0.1"), SoftwareRevision = new Version("0.0.0.1") }, knowledge);

            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, knowledge) as PortValueSingleMessage;

            // assert
            Assert.Collection(message.Data,
                d =>
                {
                    Assert.Equal(d.PortId, expectedPortId);
                    Assert.Equal(d.DataType, expectedDataType);

                    if (d is PortValueSingleMessageData<T> actual)
                    {
                        Assert.Collection(actual.InputValues, expectedData.Cast<T>().Select<T, Action<T>>(expected => actual => Assert.Equal(expected, actual)).ToArray());
                    }
                }
            );
        }
    }
}