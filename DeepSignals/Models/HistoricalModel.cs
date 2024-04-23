namespace DeepSignals.Models
{
    public class HistoricalModel : BaseClass
    {
        public DateTime _Date { get; set; }
        public double _Open { get; set; }
        public double _High { get; set; }
        public double _Low { get; set; }
        public double _Close { get; set; }
        public double _AdjClose { get; set; }
        public double _Volume { get; set; }
        public double _PriceChange { get; set; }
        public double _VolumeChange { get; set; }
        public double _VolumeIndicator { get; set; }
        public double _MA_0 { get; set; }
        public double _MA_1 { get; set; }
        public double _MA_2 { get; set; }
        public double _TRIX { get; set; }
        public double _RSI { get; set; }
    }
}
