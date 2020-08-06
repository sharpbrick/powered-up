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
    public class PortValueCombinedModeEncoderTest
    {
        [Theory]
        [InlineData("0D-00-46-00-00-07-04-06-00-00-00-95-FF", 0x00, new byte[] { 0x01, 0x02, 0x03 }, new PortModeInformationDataType[] { PortModeInformationDataType.SByte, PortModeInformationDataType.Int32, PortModeInformationDataType.Int16 }, new int[] { 4, 6, -107 })]
        public void PortValueCombinedModeEncoder_Decode(string dataAsString, byte expectedPortId, byte[] expectedModes, PortModeInformationDataType[] expectedDataType, int[] expectedData)
        {
            var knowledge = new ProtocolKnowledge();

            var serviceProvider = new ServiceCollection()
                .AddPoweredUp()
                .BuildServiceProvider();

            KnowledgeManager.ApplyDynamicProtocolKnowledge(new HubAttachedIOForAttachedDeviceMessage() { HubId = 0, IOTypeId = DeviceType.TechnicLargeLinearMotor, MessageType = MessageType.HubAttachedIO, Event = HubAttachedIOEvent.AttachedIO, PortId = 0x00, HardwareRevision = new Version("0.0.0.1"), SoftwareRevision = new Version("0.0.0.1") }, knowledge, serviceProvider.GetService<IDeviceFactory>());
            KnowledgeManager.ApplyDynamicProtocolKnowledge(new PortInputFormatSetupCombinedModeForSetModeDataSetMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.SetModeAndDataSetCombination,
                CombinationIndex = 0, // should refer 0b0000_0000_0000_1110 => SPEED POS APOS
                ModeDataSets = new PortInputFormatSetupCombinedModeModeDataSet[] {
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x01, DataSet = 0, },
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x02, DataSet = 0, },
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x03, DataSet = 0, },
                    }
            }, knowledge, serviceProvider.GetService<IDeviceFactory>());

            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, knowledge) as PortValueCombinedModeMessage;

            // assert
            Assert.Equal(expectedPortId, message.PortId);
            Assert.Collection(message.Data, Enumerable.Range(0, expectedModes.Length).Select<int, Action<PortValueData>>(pos => portValueData =>
            {
                Assert.Equal(expectedPortId, portValueData.PortId);
                Assert.Equal(expectedDataType[pos], portValueData.DataType);
                Assert.Equal(expectedDataType[pos], portValueData.DataType);

                if (portValueData is PortValueData<sbyte> actual)
                {
                    Assert.Equal(expectedData[pos], actual.InputValues[0]);
                }
                if (portValueData is PortValueData<short> actual2)
                {
                    Assert.Equal(expectedData[pos], actual2.InputValues[0]);
                }
                if (portValueData is PortValueData<int> actual3)
                {
                    Assert.Equal(expectedData[pos], actual3.InputValues[0]);
                }

            }).ToArray());
        }
    }
}