using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortInputFormatSetupCombinedModeEncoderTest
{
    [Theory]
    [InlineData("05-00-42-00-02", 0x00, PortInputFormatSetupCombinedSubCommand.LockDeviceForSetup)]
    [InlineData("05-00-42-00-03", 0x00, PortInputFormatSetupCombinedSubCommand.UnlockAndStartWithMultiUpdateEnabled)]
    [InlineData("05-00-42-00-04", 0x00, PortInputFormatSetupCombinedSubCommand.UnlockAndStartWithMultiUpdateDisabled)]
    [InlineData("05-00-42-00-06", 0x00, PortInputFormatSetupCombinedSubCommand.ResetSensor)]
    public void PortInputFormatSetupCombinedModeEncoder_Encode_SimpleCommands(string expectedDataAsString, byte portId, PortInputFormatSetupCombinedSubCommand subCommand)
    {
        // arrange
        var message = new PortInputFormatSetupCombinedModeMessage(portId, subCommand);

        // act
        var data = MessageEncoder.Encode(message, null);

        // assert
        Assert.Equal(expectedDataAsString, BytesStringUtil.DataToString(data));
    }

    [Theory]
    [InlineData("09-00-42-00-01-00-10-20-30", 0x00, new byte[] { 0x10, 0x20, 0x30 })]
    public void PortInputFormatSetupCombinedModeEncoder_Encode_ModeDataSetCommands(string expectedDataAsString, byte portId, byte[] modeDataSets)
    {
        // arrange
        var message = new PortInputFormatSetupCombinedModeForSetModeDataSetMessage(portId, 0,
            modeDataSets.Select(b => new PortInputFormatSetupCombinedModeModeDataSet(
                Mode: (byte)((b & 0xF0) >> 4),
                DataSet: (byte)(b & 0x0F)
            )).ToArray()
        );

        // act
        var data = MessageEncoder.Encode(message, null);

        // assert
        Assert.Equal(expectedDataAsString, BytesStringUtil.DataToString(data));
    }
}
