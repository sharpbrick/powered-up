using System;
using System.Collections.Generic;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortOutputCommandFeedbackEncoder : IMessageContentEncoder
{
    public ushort CalculateContentLength(LegoWirelessMessage message)
        => throw new NotImplementedException();

    public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
    {
        var remainingSlice = data;

        var result = new List<PortOutputCommandFeedback>();

        while (remainingSlice.Length >= 2)
        {
            var feedback = new PortOutputCommandFeedback(remainingSlice[0], (PortFeedback)remainingSlice[1]);

            result.Add(feedback);
            remainingSlice = remainingSlice[2..];
        }

        return new PortOutputCommandFeedbackMessage(result.ToArray());
    }

    public void Encode(LegoWirelessMessage message, in Span<byte> data)
        => throw new NotImplementedException();
}
