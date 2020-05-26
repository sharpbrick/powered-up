namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.9.1
    public class ErrorMessage : CommonMessageHeader
    {
        public byte CommandType { get; set; }
        public byte ErrorCode { get; set; }
    }
}