namespace SharpBrick.PoweredUp
{
    public class Value<TDatasetType, TOutputType>
    {
        public TDatasetType Raw { get; set; }
        public TOutputType SI { get; set; }
        public TOutputType Pct { get; set; }
    }
}