
using DeepSignals.Models;
using Google.Protobuf;
using Polly.Retry;
using System.Net.WebSockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

using Polly;
using Polly.Retry;

namespace DeepSignals.Settings.Helpers
{
    public class WebSocketHelper
    {
  


        public static PriceData DecodePricingData(byte[] data)
        {
            var pricingData = new PriceData();
            var reader = new CodedInputStream(data);

            while (!reader.IsAtEnd)
            {
                uint tag = reader.ReadTag();
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1:
                        pricingData.Id = reader.ReadString();
                        break;
                    case 2:




                        pricingData.Price = reader.ReadFloat();


                        if (pricingData.Price.ToString().Contains("E"))
                        {
                            pricingData.PriceString = pricingData.Price.ToString("0.############");
                        }
                        else
                        {
                            pricingData.PriceString = pricingData.Price.ToString();
                        }


                        break;
                    case 3:
                        pricingData.Time = reader.ReadInt64();
                        pricingData.TimeDT = DateTime.Now;
                        break;
                    case 4:
                        pricingData.Currency = reader.ReadString();
                        break;
                    case 5:
                        pricingData.Exchange = reader.ReadString();
                        break;
                    case 6:
                        pricingData.QuotesType = reader.ReadInt32();
                        pricingData.QuotesString = ((PriceData.Quote)pricingData.QuotesType).ToString();
                        break;
                    case 7:
                        pricingData.MarketHoursType = reader.ReadInt32();
                        pricingData.MarketHoursString = ((PriceData.MarketHours)pricingData.MarketHoursType).ToString();
                        break;
                    case 8:
                        pricingData.ChangePercent = reader.ReadFloat();
                        break;
                    case 9:
                        pricingData.DayVolume = reader.ReadSInt64();
                        break;
                    case 10:
                        pricingData.DayHigh = reader.ReadFloat();
                        break;
                    case 11:
                        pricingData.DayLow = reader.ReadFloat();
                        break;
                    case 12:
                        pricingData.Change = reader.ReadFloat();
                        break;
                    case 13:
                        pricingData.ShortName = reader.ReadString();
                        break;
                    case 14:
                        pricingData.ExpireDate = reader.ReadSInt64();
                        break;
                    case 15:
                        pricingData.OpenPrice = reader.ReadFloat();
                        break;
                    case 16:
                        pricingData.PreviousClose = reader.ReadFloat();
                        break;
                    case 17:
                        pricingData.StrikePrice = reader.ReadFloat();
                        break;
                    case 18:
                        pricingData.UnderlyingSymbol = reader.ReadString();
                        break;
                    case 19:
                        pricingData.OpenInterest = reader.ReadSInt64();
                        break;
                    case 20:
                        pricingData.OptionsType = reader.ReadSInt64();
                        pricingData.OptionsString = ((PriceData.Option)pricingData.OptionsType).ToString();

                        break;
                    case 21:
                        pricingData.MiniOption = reader.ReadSInt64();
                        break;
                    case 22:
                        pricingData.LastSize = reader.ReadSInt64();
                        break;
                    case 23:
                        pricingData.Bid = reader.ReadFloat();
                        break;
                    case 24:
                        pricingData.BidSize = reader.ReadSFixed64();
                        break;
                    case 25:
                        pricingData.Ask = reader.ReadFloat();
                        break;
                    case 26:
                        pricingData.AskSize = reader.ReadSInt64();
                        break;
                    case 27:
                        pricingData.PriceHint = reader.ReadSInt64();
                        break;
                    case 28:
                        pricingData.Vol24hr = reader.ReadSInt64();
                        break;
                    case 29:
                        pricingData.VolAllCurrencies = reader.ReadSInt64();
                        break;
                    case 30:
                        pricingData.FromCurrency = reader.ReadString();
                        break;
                    case 31:
                        pricingData.LastMarket = reader.ReadString();
                        break;
                    case 32:
                        pricingData.CirculatingSupply = reader.ReadDouble();
                        break;
                    case 33:
                        pricingData.MarketCap = reader.ReadDouble();
                        break;
                    default:
                        reader.SkipLastField();
                        break;
                }
            }

            return pricingData;
        }
    }
}