namespace DeepSignals.Models
{
    public class Candlestick
    {
        public float OpenPrice { get; set; }
        public float ClosePrice { get; set; }
        public float MaxPrice { get; set; }
        public float MinPrice { get; set; }
        public float CurrentPrice { get; set; }
        public DateTime Minute { get; set; }

        public string Move { get; set; }
    }
}
