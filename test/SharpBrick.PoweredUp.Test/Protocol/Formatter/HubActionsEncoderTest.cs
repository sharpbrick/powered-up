using System.Globalization;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubActionsEncoderTest
    {
        [Theory]
        [InlineData(HubAction.SwitchOffHub, "04-00-02-01")]
        [InlineData(HubAction.Disconnect, "04-00-02-02")]
        [InlineData(HubAction.VccPortControlOn, "04-00-02-03")]
        [InlineData(HubAction.VccPortControlOff, "04-00-02-04")]
        [InlineData(HubAction.ActivateBusyIndication, "04-00-02-05")]
        [InlineData(HubAction.ResetBusyIndication, "04-00-02-06")]
        [InlineData(HubAction.ProductionShutdown, "04-00-02-2F")]
        [InlineData(HubAction.HubWillSwitchOff, "04-00-02-30")]
        [InlineData(HubAction.HubWillDisconnect, "04-00-02-31")]
        [InlineData(HubAction.HubWillGoIntoBootMode, "04-00-02-32")]
        public void HubActionsEncoder_Encode(HubAction action, string expectedData)
        {
            // arrange
            var message = new HubActionMessage(action);

            // act
            var data = MessageEncoder.Encode(message, null);

            // assert
            Assert.Equal(expectedData, BytesStringUtil.DataToString(data));
        }

        [Theory]
        [InlineData(HubAction.SwitchOffHub, "04-00-02-01")]
        [InlineData(HubAction.Disconnect, "04-00-02-02")]
        [InlineData(HubAction.VccPortControlOn, "04-00-02-03")]
        [InlineData(HubAction.VccPortControlOff, "04-00-02-04")]
        [InlineData(HubAction.ActivateBusyIndication, "04-00-02-05")]
        [InlineData(HubAction.ResetBusyIndication, "04-00-02-06")]
        [InlineData(HubAction.ProductionShutdown, "04-00-02-2F")]
        [InlineData(HubAction.HubWillSwitchOff, "04-00-02-30")]
        [InlineData(HubAction.HubWillDisconnect, "04-00-02-31")]
        [InlineData(HubAction.HubWillGoIntoBootMode, "04-00-02-32")]
        public void HubActionsEncoder_Decode(HubAction expectedAction, string dataAsString)
        {
            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, null) as HubActionMessage;

            // assert
            Assert.Equal(expectedAction, message.Action);
        }
    }
}