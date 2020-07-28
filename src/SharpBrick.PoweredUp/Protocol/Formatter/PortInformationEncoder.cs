using System;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInformationEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
        {
            byte portId = data[0];
            var informationType = (PortInformationType)data[1];

            PortInformationMessage result = null;

            switch (informationType)
            {
                case PortInformationType.ModeInfo:
                    result = new PortInformationForModeInfoMessage()
                    {
                        PortId = portId,
                        InformationType = informationType,

                        // capabilities
                        OutputCapability = (data[2] & 0x01) == 0x01, // seen from hub
                        InputCapability = (data[2] & 0x02) == 0x02, // seen from hub
                        LogicalCombinableCapability = (data[2] & 0x04) == 0x04,
                        LogicalSynchronizableCapability = (data[2] & 0x08) == 0x08,

                        TotalModeCount = data[3],
                        InputModes = BitConverter.ToUInt16(data.Slice(4, 2)),
                        OutputModes = BitConverter.ToUInt16(data.Slice(6, 2)),
                    };
                    break;

                case PortInformationType.PossibleModeCombinations:
                    var numberOfModes = (int)((data.Length - 2) / 2);
                    ushort[] combinations = new ushort[numberOfModes];

                    for (int idx = 0; idx < numberOfModes; idx++)
                    {
                        combinations[idx] = BitConverter.ToUInt16(data.Slice(2 + idx * 2, 2));
                    }

                    result = new PortInformationForPossibleModeCombinationsMessage()
                    {
                        PortId = portId,
                        InformationType = informationType,

                        ModeCombinations = combinations,
                    };
                    break;
            }

            return result;
        }

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}