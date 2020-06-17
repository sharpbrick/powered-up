namespace SharpBrick.PoweredUp
{
    public class Value<TPayload>
    {
        public TPayload Raw { get; set; }
        public TPayload SI { get; set; }
        public TPayload Pct { get; set; }
    }
}