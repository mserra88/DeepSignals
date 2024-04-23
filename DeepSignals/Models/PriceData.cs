namespace DeepSignals.Models
{
    public class PriceData
    {

        public enum Quote
        {
            NONE = 0,
            ALTSYMBOL = 5,
            HEARTBEAT = 7,
            EQUITY = 8,
            INDEX = 9,
            MUTUALFUND = 11,
            MONEYMARKET = 12,
            OPTION = 13,
            CURRENCY = 14,
            WARRANT = 15,
            BOND = 17,
            FUTURE = 18,
            ETF = 20,
            COMMODITY = 23,
            ECNQUOTE = 28,
            CRYPTOCURRENCY = 41,
            INDICATOR = 42,
            INDUSTRY = 1000
        }

        public enum Option
        {
            CALL = 0,
            PUT = 1
        }

        public enum MarketHours
        {
            PRE_MARKET = 0,
            REGULAR_MARKET = 1,
            POST_MARKET = 2,
            EXTENDED_HOURS_MARKET = 3
        }



        public string Id { get; set; }
        public float Price { get; set; }

        public string PriceString { get; set; }


        public long Time { get; set; }

        public DateTime TimeDT { get; set; }

        public string Currency { get; set; }
        public string Exchange { get; set; }
        public int QuotesType { get; set; }
        public string QuotesString { get; set; }

        public int MarketHoursType { get; set; }

        public string MarketHoursString { get; set; }

        public float ChangePercent { get; set; }
        public Int64 DayVolume { get; set; }
        public float DayHigh { get; set; }
        public float DayLow { get; set; }
        public float Change { get; set; }
        public string ShortName { get; set; }
        public Int64 ExpireDate { get; set; }
        public float OpenPrice { get; set; }
        public float PreviousClose { get; set; }
        public float StrikePrice { get; set; }
        public string UnderlyingSymbol { get; set; }
        public Int64 OpenInterest { get; set; }
        public Int64 OptionsType { get; set; }
        public string OptionsString { get; set; }

        public Int64 MiniOption { get; set; }
        public Int64 LastSize { get; set; }
        public float Bid { get; set; }
        public Int64 BidSize { get; set; }
        public float Ask { get; set; }
        public Int64 AskSize { get; set; }
        public Int64 PriceHint { get; set; }
        public Int64 Vol24hr { get; set; }
        public Int64 VolAllCurrencies { get; set; }
        public string FromCurrency { get; set; }
        public string LastMarket { get; set; }
        public double CirculatingSupply { get; set; }
        public double MarketCap { get; set; }
    }
}
