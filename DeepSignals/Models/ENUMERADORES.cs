namespace DeepSignals.Models
{
    /*
    public enum As
    {
        Table,
        Select,
        SelectNavigate,
        AutocompleteInput,
        ChartList,
        ChartMarquee
    }
    */
    public enum SelectType
    {
        Default,
        Popular
    }

    public enum LocationState
    {
        Home,
        Markets,
        Ticker,
        Movers,
        NotFound,

        Type,
        Sector,
        Exchange,
        Popular
    }


    public enum Type
    {
        Currencies,
        Stocks,
        Indices,
        Cryptocurrencies
    }

    public enum Stocks
    {
        ConsumerGods,
        Services
    }

    public enum Cryptocurrencies
    {
        Cryptocrosses,
    }

    public enum Exchanges
    {
        Nasdaq,
        NYSE,
        London
    }

    public enum ExchangesCripto
    {
        Cuz,
        Bit
    }

    public static class MappedDictionary
    {
        public static Dictionary<Enum, (string Type, Dictionary<Enum, string> Sector, Dictionary<Enum, string> Exchange)> StateToInfo = new Dictionary<Enum, (string, Dictionary<Enum, string>, Dictionary<Enum, string>)>
        {
            {
                LocationState.NotFound, ("Not Found", null, null)
            },
            {
                LocationState.Home, (nameof(LocationState.Home), null, null)
            },
            {
                LocationState.Markets, (nameof(LocationState.Markets), null, null)
            },
            {
                LocationState.Ticker, (nameof(LocationState.Ticker), null, null)
            },
            {
                LocationState.Movers, ("Market Movers", null, null)
            },
            {
                Type.Currencies, (nameof(Type.Currencies), null, null)
            },
            {
                Type.Indices, (nameof(Type.Indices), null, null)
            },
            {
                Type.Stocks, ("SockS", 
                
                new Dictionary<Enum, string>
                {
                    { Stocks.ConsumerGods, "Consumer Gods" },
                    { Stocks.Services, nameof(Stocks.Services) }
                },
                new Dictionary<Enum, string>
                {
                    { Exchanges.Nasdaq, "NasDaq" },
                    { Exchanges.NYSE, nameof(Exchanges.NYSE) },
                    { Exchanges.London, nameof(Exchanges.London) }
                })
            },
            {
                Type.Cryptocurrencies, (nameof(Type.Cryptocurrencies), 
                new Dictionary<Enum, string>
                {
                    //sector  a obtener
                    { Cryptocurrencies.Cryptocrosses, nameof(Cryptocurrencies.Cryptocrosses) }

                },
                new Dictionary<Enum, string>
                {
                    { ExchangesCripto.Cuz, nameof(ExchangesCripto.Cuz) },
                    { ExchangesCripto.Bit, "B1T" }
                })
            },
        };
    }
}





























/*
   public static class LocationStateMapperConfig
   {
       public static Dictionary<LocationState, LocationStateInfo> StateToInfo = new Dictionary<LocationState, LocationStateInfo>
       {
           { LocationState.Home, new LocationStateInfo { Title = LocationState.Home.ToString(), URL = "/" } },
           { LocationState.Markets, new LocationStateInfo { Title = LocationState.Markets.ToString(), URL = "/markets" } },
           { LocationState.Ticker, null },

           { LocationState.Type, null },
           { LocationState.Currencies, new LocationStateInfo { Title = LocationState.Currencies.ToString(), URL = "/currencies" } },
           { LocationState.Stocks, new LocationStateInfo { Title = LocationState.Stocks.ToString(), URL = "/stocks" } },
           { LocationState.Indices, new LocationStateInfo { Title = LocationState.Indices.ToString(), URL = "/indices" } },
           { LocationState.Cryptocurrencies, new LocationStateInfo { Title = LocationState.Cryptocurrencies.ToString(), URL = "/cryptocurrencies" } },

           { LocationState.Sector, null },
           { LocationState.ConsumerGoods, new LocationStateInfo { Title = "Consumer Goods", URL = "/consumergoods" } },
           { LocationState.Services, new LocationStateInfo { Title = LocationState.Services.ToString(), URL = "/services" } },
           { LocationState.Cryptocrosses, new LocationStateInfo { Title = "Crypto Crosses", URL = "/cryptocrosses" } },

           { LocationState.NotFound, new LocationStateInfo { Title = "Not Found", URL = "/NotFound" } },


           // Agrega más estados y sus información correspondiente aquí
       };
   }
   */

/*
public static class TickerSectorMapperConfig
{
    public static Dictionary<TickerType, Type> typeToSectorEnum = new Dictionary<TickerType, Type>
    {
        { TickerType.Stocks, typeof(StocksSector) },
        { TickerType.Cryptocurrencies, typeof(CryptocurrenciesSector) },
        //....
        //...
        // Agrega más tipos aquí si es necesario
    };
}
*/
