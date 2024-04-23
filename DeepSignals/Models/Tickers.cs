namespace DeepSignals.Models
{
    public class Tickers : BaseClass
    {
        public string alias { get; set; }

        public string type { get; set; }

        public string sector { get; set; } = "";

        public string exchange { get; set; } = "";

        public string popular { get; set; }

        public bool featured { get; set; }
    }
}
