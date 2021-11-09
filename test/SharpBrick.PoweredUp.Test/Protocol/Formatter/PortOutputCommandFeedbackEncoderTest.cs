using System;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortOutputCommandFeedbackEncoderTest
{
    [Theory]
    [InlineData("05-00-82-32-0A", new byte[] { 50 }, new PortFeedback[] { PortFeedback.Idle | PortFeedback.BufferEmptyAndCommandCompleted })]
    [InlineData("05-00-82-32-0A-33-02", new byte[] { 50, 51 }, new PortFeedback[] { PortFeedback.Idle | PortFeedback.BufferEmptyAndCommandCompleted, PortFeedback.BufferEmptyAndCommandCompleted })]
    public void PortOutputCommandFeedbackEncoder_Decode(string dataAsString, byte[] ports, PortFeedback[] feedbacks)
    {
        var data = BytesStringUtil.StringToData(dataAsString);

        var message = MessageEncoder.Decode(data, null) as PortOutputCommandFeedbackMessage;

        Assert.NotNull(message);
        Assert.Collection(message.Feedbacks, Enumerable.Range(0, ports.Length).Select<int, Action<PortOutputCommandFeedback>>(pos => feedback =>
        {
            Assert.Equal(ports[pos], feedback.PortId);
            Assert.Equal(feedbacks[pos], feedback.Feedback);
        }).ToArray());
    }
}
