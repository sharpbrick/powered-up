using System;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubPropertiesEncoderTest
    {
        [Theory]
        [InlineData("06-00-01-02-06-00", HubProperty.Button, HubPropertyOperation.Update, false)]
        [InlineData("06-00-01-02-06-01", HubProperty.Button, HubPropertyOperation.Update, true)]
        [InlineData("06-00-01-05-06-61", HubProperty.Rssi, HubPropertyOperation.Update, (sbyte)97)]
        [InlineData("06-00-01-06-06-64", HubProperty.BatteryVoltage, HubPropertyOperation.Update, (byte)100)]
        [InlineData("06-00-01-07-06-00", HubProperty.BatteryType, HubPropertyOperation.Update, BatteryType.Normal)]
        [InlineData("06-00-01-07-06-01", HubProperty.BatteryType, HubPropertyOperation.Update, BatteryType.RechargeableBlock)]
        [InlineData("06-00-01-0B-06-80", HubProperty.SystemTypeId, HubPropertyOperation.Update, SystemType.LegoTechnic_MediumHub)]
        [InlineData("06-00-01-0C-06-00", HubProperty.HardwareNetworkId, HubPropertyOperation.Update, (byte)0)]
        //[InlineData("0B-00-01-0D-06-90-84-2B-49-5D-19", HubProperty.PrimaryMacAddress, HubPropertyOperation.Update)]
        //[InlineData("0B-00-01-0E-06-90-84-2B-83-5D-19", HubProperty.SecondaryMacAddress, HubPropertyOperation.Update, (byte)0)]
        // HardwareNetworkFamily threw error => UNSPECED
        public void HubPropertiesEncoder_Decode_UpdateUpstream<T>(string messageAsString, HubProperty expectedProperty, HubPropertyOperation expectedPropertyOperation, T payload)
        {
            // arrange
            var data = BytesStringUtil.StringToData(messageAsString).AsSpan()[3..];

            // act
            var message = new HubPropertiesEncoder().Decode(0x00, data) as HubPropertyMessage<T>;

            // assert
            Assert.Equal(expectedProperty, message.Property);
            Assert.Equal(expectedPropertyOperation, message.Operation);
            Assert.Equal(payload, message.Payload);
        }

        [Theory]
        [InlineData("09-00-01-03-06-00-00-00-11", HubProperty.FwVersion, HubPropertyOperation.Update, "1.1.0.0")]
        [InlineData("09-00-01-04-06-00-00-00-07", HubProperty.HwVersion, HubPropertyOperation.Update, "0.7.0.0")]
        [InlineData("07-00-01-0A-06-00-03", HubProperty.LegoWirelessProtocolVersion, HubPropertyOperation.Update, "3.0")]
        public void HubPropertiesEncoder_Decode_UpdateUpstream_VersionShim(string messageAsString, HubProperty expectedProperty, HubPropertyOperation expectedPropertyOperation, string payload)
            => HubPropertiesEncoder_Decode_UpdateUpstream(messageAsString, expectedProperty, expectedPropertyOperation, new Version(payload));

        [Theory]
        [InlineData("10-00-01-01-06-54-65-63-68-6E-69-63-20-48-75-62", HubProperty.AdvertisingName, HubPropertyOperation.Update, "Technic Hub")]
        [InlineData("14-00-01-08-06-4C-45-47-4F-20-53-79-73-74-65-6D-20-41-2F-53", HubProperty.ManufacturerName, HubPropertyOperation.Update, "LEGO System A/S")]
        [InlineData("11-00-01-09-06-33-5F-30-32-5F-30-30-5F-76-31-2E-31", HubProperty.RadioFirmwareVersion, HubPropertyOperation.Update, "3_02_00_v1.1")]
        public void HubPropertiesEncoder_Decode_UpdateUpstream_StringShim(string messageAsString, HubProperty expectedProperty, HubPropertyOperation expectedPropertyOperation, string payload)
            => HubPropertiesEncoder_Decode_UpdateUpstream(messageAsString, expectedProperty, expectedPropertyOperation, payload);


        [Theory]
        [InlineData(HubProperty.Button, HubPropertyOperation.RequestUpdate, "05-00-01-02-05")]
        [InlineData(HubProperty.Button, HubPropertyOperation.EnableUpdates, "05-00-01-02-02")]
        [InlineData(HubProperty.Button, HubPropertyOperation.DisableUpdates, "05-00-01-02-03")]
        public void HubPropertiesEncoder_Encode_Downstream(HubProperty property, HubPropertyOperation operation, string expectedData)
        {
            // act
            var data = MessageEncoder.Encode(new HubPropertyMessage(property, operation), null);

            // assert
            Assert.Equal(expectedData, BytesStringUtil.DataToString(data));
        }
    }
}