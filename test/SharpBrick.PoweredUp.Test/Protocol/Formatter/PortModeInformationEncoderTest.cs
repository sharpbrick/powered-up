using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortModeInformationEncoderTest
    {
        [Theory]
        // Technic Large Motor
        [InlineData("11-00-44-00-00-00-50-4F-57-45-52-00-00-00-00-00-00", 0x00, 0x00, PortModeInformationType.Name, "POWER")]
        [InlineData("11-00-44-00-01-00-53-50-45-45-44-00-00-00-00-00-00", 0x00, 0x01, PortModeInformationType.Name, "SPEED")]
        [InlineData("11-00-44-00-02-00-50-4F-53-00-00-00-00-00-00-00-00", 0x00, 0x02, PortModeInformationType.Name, "POS")]
        [InlineData("11-00-44-00-03-00-41-50-4F-53-00-00-00-00-00-00-00", 0x00, 0x03, PortModeInformationType.Name, "APOS")]
        [InlineData("11-00-44-00-04-00-4C-4F-41-44-00-00-00-00-00-00-00", 0x00, 0x04, PortModeInformationType.Name, "LOAD")]
        [InlineData("11-00-44-00-05-00-43-41-4C-49-42-00-00-00-00-00-00", 0x00, 0x05, PortModeInformationType.Name, "CALIB")]
        public void PortModeInformationEncoder_Decode_Name(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType, string expectedText)
        {
            var message = Decode<PortModeInformationForNameMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedText, message.Name);
        }

        [Theory]
        // Technic Large Motor
        [InlineData("0E-00-44-00-00-01-00-00-C8-C2-00-00-C8-42", 0x00, 0x00, PortModeInformationType.Raw, -100, 100)]
        [InlineData("0E-00-44-00-01-01-00-00-C8-C2-00-00-C8-42", 0x00, 0x01, PortModeInformationType.Raw, -100, 100)]
        [InlineData("0E-00-44-00-02-01-00-00-B4-C3-00-00-B4-43", 0x00, 0x02, PortModeInformationType.Raw, -360, 360)]
        [InlineData("0E-00-44-00-03-01-00-00-B4-C3-00-00-B4-43", 0x00, 0x03, PortModeInformationType.Raw, -360, 360)]
        [InlineData("0E-00-44-00-04-01-00-00-00-00-00-00-FE-42", 0x00, 0x04, PortModeInformationType.Raw, 0, 127)]
        [InlineData("0E-00-44-00-05-01-00-00-00-00-00-00-00-44", 0x00, 0x05, PortModeInformationType.Raw, 0, 512)]
        public void PortModeInformationEncoder_Decode_Raw(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType, float expectedMin, float expectedMax)
        {
            var message = Decode<PortModeInformationForRawMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedMin, message.RawMin);
            Assert.Equal(expectedMax, message.RawMax);
        }

        [Theory]
        // Technic Large Motor
        [InlineData("0E-00-44-00-00-02-00-00-C8-C2-00-00-C8-42", 0x00, 0x00, PortModeInformationType.Pct, -100, 100)]
        [InlineData("0E-00-44-00-01-02-00-00-C8-C2-00-00-C8-42", 0x00, 0x01, PortModeInformationType.Pct, -100, 100)]
        [InlineData("0E-00-44-00-02-02-00-00-C8-C2-00-00-C8-42", 0x00, 0x02, PortModeInformationType.Pct, -100, 100)]
        [InlineData("0E-00-44-00-03-02-00-00-C8-C2-00-00-C8-42", 0x00, 0x03, PortModeInformationType.Pct, -100, 100)]
        [InlineData("0E-00-44-00-04-02-00-00-00-00-00-00-C8-42", 0x00, 0x04, PortModeInformationType.Pct, 0, 100)]
        [InlineData("0E-00-44-00-05-02-00-00-00-00-00-00-C8-42", 0x00, 0x05, PortModeInformationType.Pct, 0, 100)]
        public void PortModeInformationEncoder_Decode_Pct(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType, float expectedMin, float expectedMax)
        {
            var message = Decode<PortModeInformationForPctMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedMin, message.PctMin);
            Assert.Equal(expectedMax, message.PctMax);
        }

        [Theory]
        // Technic Large Motor
        [InlineData("0E-00-44-00-00-03-00-00-C8-C2-00-00-C8-42", 0x00, 0x00, PortModeInformationType.SI, -100, 100)]
        [InlineData("0E-00-44-00-01-03-00-00-C8-C2-00-00-C8-42", 0x00, 0x01, PortModeInformationType.SI, -100, 100)]
        [InlineData("0E-00-44-00-02-03-00-00-B4-C3-00-00-B4-43", 0x00, 0x02, PortModeInformationType.SI, -360, 360)]
        [InlineData("0E-00-44-00-03-03-00-00-B4-C3-00-00-B4-43", 0x00, 0x03, PortModeInformationType.SI, -360, 360)]
        [InlineData("0E-00-44-00-04-03-00-00-00-00-00-00-FE-42", 0x00, 0x04, PortModeInformationType.SI, 0, 127)]
        [InlineData("0E-00-44-00-05-03-00-00-00-00-00-00-00-44", 0x00, 0x05, PortModeInformationType.SI, 0, 512)]
        public void PortModeInformationEncoder_Decode_SI(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType, float expectedMin, float expectedMax)
        {
            var message = Decode<PortModeInformationForSIMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedMin, message.SIMin);
            Assert.Equal(expectedMax, message.SIMax);
        }

        [Theory]
        // Technic Large Motor
        [InlineData("0A-00-44-00-00-04-50-43-54-00", 0x00, 0x00, PortModeInformationType.Symbol, "PCT")]
        [InlineData("0A-00-44-00-01-04-50-43-54-00", 0x00, 0x01, PortModeInformationType.Symbol, "PCT")]
        [InlineData("0A-00-44-00-02-04-44-45-47-00", 0x00, 0x02, PortModeInformationType.Symbol, "DEG")]
        [InlineData("0A-00-44-00-03-04-44-45-47-00", 0x00, 0x03, PortModeInformationType.Symbol, "DEG")]
        [InlineData("0A-00-44-00-04-04-50-43-54-00", 0x00, 0x04, PortModeInformationType.Symbol, "PCT")]
        [InlineData("0A-00-44-00-05-04-52-41-57-00", 0x00, 0x05, PortModeInformationType.Symbol, "RAW")]
        public void PortModeInformationEncoder_Decode_Symbol(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType, string expectedText)
        {
            var message = Decode<PortModeInformationForSymbolMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedText, message.Symbol);
        }

        [Theory]
        // Technic Large Motor
        [InlineData("08-00-44-00-00-05-00-10", 0x00, 0x00, PortModeInformationType.Mapping, false, false, false, false, false, false, false, true, false, false)] // power only supports output abs values
        [InlineData("08-00-44-00-01-05-10-10", 0x00, 0x01, PortModeInformationType.Mapping, false, false, true, false, false, false, false, true, false, false)] // power supports output and input abs values
        [InlineData("08-00-44-00-02-05-08-08", 0x00, 0x02, PortModeInformationType.Mapping, false, false, false, true, false, false, false, false, true, false)] // pos only relative input and output
        [InlineData("08-00-44-00-03-05-08-08", 0x00, 0x03, PortModeInformationType.Mapping, false, false, false, true, false, false, false, false, true, false)] // apos only relative input and output
        [InlineData("08-00-44-00-04-05-08-08", 0x00, 0x04, PortModeInformationType.Mapping, false, false, false, true, false, false, false, false, true, false)] // load only relative input and output
        [InlineData("08-00-44-00-05-05-00-00", 0x00, 0x05, PortModeInformationType.Mapping, false, false, false, false, false, false, false, false, false, false)] // calib does not support anything?
        public void PortModeInformationEncoder_Decode_Mapping(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType,
             bool expectedInputNull, bool expectedInputMapping20, bool expectedInputAbs, bool expectedInputRel, bool expectedInputDis,
             bool expectedOutputNull, bool expectedOutputMapping20, bool expectedOutputAbs, bool expectedOutputRel, bool expectedOutputDis)
        {
            var message = Decode<PortModeInformationForMappingMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedInputNull, message.InputSupportsNull);
            Assert.Equal(expectedInputMapping20, message.InputSupportFunctionalMapping20);
            Assert.Equal(expectedInputAbs, message.InputAbsolute);
            Assert.Equal(expectedInputRel, message.InputRelative);
            Assert.Equal(expectedInputDis, message.InputDiscrete);

            Assert.Equal(expectedOutputNull, message.OutputSupportsNull);
            Assert.Equal(expectedOutputMapping20, message.OutputSupportFunctionalMapping20);
            Assert.Equal(expectedOutputAbs, message.OutputAbsolute);
            Assert.Equal(expectedOutputRel, message.OutputRelative);
            Assert.Equal(expectedOutputDis, message.OutputDiscrete);
        }


        [Theory]
        // Technic Large Motor
        [InlineData("0A-00-44-00-00-80-01-00-01-00", 0x00, 0x00, PortModeInformationType.ValueFormat, 1, PortModeInformationDataType.SByte, 1, 0)]
        [InlineData("0A-00-44-00-01-80-01-00-04-00", 0x00, 0x01, PortModeInformationType.ValueFormat, 1, PortModeInformationDataType.SByte, 4, 0)]
        [InlineData("0A-00-44-00-02-80-01-02-04-00", 0x00, 0x02, PortModeInformationType.ValueFormat, 1, PortModeInformationDataType.Int32, 4, 0)]
        [InlineData("0A-00-44-00-03-80-01-01-03-00", 0x00, 0x03, PortModeInformationType.ValueFormat, 1, PortModeInformationDataType.Int16, 3, 0)]
        [InlineData("0A-00-44-00-04-80-01-00-01-00", 0x00, 0x04, PortModeInformationType.ValueFormat, 1, PortModeInformationDataType.SByte, 1, 0)]
        [InlineData("0A-00-44-00-05-80-03-01-03-00", 0x00, 0x05, PortModeInformationType.ValueFormat, 3, PortModeInformationDataType.Int16, 3, 0)]
        public void PortModeInformationEncoder_Decode_ValueFormat(string data, byte expectedPort, byte expectedMode, PortModeInformationType expectedType, byte expectedNr, PortModeInformationDataType expectedDataType, byte expectedFigures, byte expectedDecimals)
        {
            var message = Decode<PortModeInformationForValueFormatMessage>(data);

            // assert
            DefaultAsserts(expectedPort, expectedMode, expectedType, message);
            Assert.Equal(expectedNr, message.NumberOfDatasets);
            Assert.Equal(expectedDataType, message.DatasetType);
            Assert.Equal(expectedFigures, message.TotalFigures);
            Assert.Equal(expectedDecimals, message.Decimals);
        }

        private static void DefaultAsserts(byte expectedPort, byte expectedMode, PortModeInformationType expectedType, PortModeInformationMessage message)
        {
            Assert.NotNull(message);

            Assert.Equal(expectedPort, message.PortId);
            Assert.Equal(expectedMode, message.Mode);
            Assert.Equal(expectedType, message.InformationType);
        }

        private TMessage Decode<TMessage>(string dataAsString) where TMessage : class
        {
            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            TMessage result = MessageEncoder.Decode(data, null) as TMessage;

            return result;
        }
    }
}