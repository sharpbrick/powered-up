using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Knowledge;
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
            => PortValueSingleEncoder_Decode<short, short>(dataAsString, expectedPortId, expectedDataType, expectedData);

        private void PortValueSingleEncoder_Decode<TDatasetType, TOutputType>(string dataAsString, byte expectedPortId, PortModeInformationDataType expectedDataType, TDatasetType[] expectedData)
        {
            var knowledge = new ProtocolKnowledge();

            var serviceProvider = new ServiceCollection()
                .AddPoweredUp()
                .BuildServiceProvider();


            KnowledgeManager.ApplyDynamicProtocolKnowledge(new HubAttachedIOForAttachedDeviceMessage(0x63, DeviceType.TechnicMediumHubTiltSensor, new Version("0.0.0.1"), new Version("0.0.0.1")) { HubId = 0 }, knowledge, serviceProvider.GetService<IDeviceFactory>());

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

                    if (d is PortValueData<TDatasetType, TOutputType> actual)
                    {
                        Assert.Collection(actual.InputValues, expectedData.Cast<TDatasetType>().Select<TDatasetType, Action<TDatasetType>>(expected => actual => Assert.Equal(expected, actual)).ToArray());
                    }
                }
            );
        }
    }
}