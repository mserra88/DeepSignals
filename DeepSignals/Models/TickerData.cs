namespace DeepSignals.Models
{
    public class TickerData
    {
        public PriceData CurrentPriceData { get; set; }
        public Candlestick CurrentCandle { get; set; }
        public DateTime DateTimeNow { get; set; }
    }
}
