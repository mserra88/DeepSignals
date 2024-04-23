using DeepSignals.Models;
using DeepSignals.Settings.Helpers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace DeepSignals.Services
{
    public class TickerDataService
    {

        private static readonly Dictionary<string, TickerData> _tickerDataMap = new Dictionary<string, TickerData>();
        private static Dictionary<string, Candlestick> candlesData = new Dictionary<string, Candlestick>();
        private object _priceDataLock = new object();


        public TickerData GetTickerData(string ticker)
        {
            // Retrieve the ticker data from the dictionary
            if (_tickerDataMap.TryGetValue(ticker, out var data))
            {
                return data;
            }
            else
            {
                // Return null or handle the case when the ticker data is not found
                return null;
            }
        }



        public void SetTickerData(List<string> _tickers, List<PriceData> priceDataList)
        {

            foreach (var ticker in _tickers)
            {
             
                            Candlestick currentCandle = new Candlestick();
                            PriceData currentPriceData = new PriceData();

                            lock (_priceDataLock)
                            {
                                if (priceDataList.Count > 0)
                                {
                                    currentPriceData = priceDataList
                                       .Where(item => item.Id == ticker)
                                       .ToList()                                                //TOLIST IMPORTANTE
                                       .LastOrDefault();
                                }
                            }

                            if (currentPriceData?.Price != null)
                            {
                                DateTime currentMinute = DateTime.Now.AddSeconds(-DateTime.Now.Second);

                                foreach (var candle in candlesData)
                                {
                                    if (candle.Key == ticker && candle.Value.Minute.ToString() == currentMinute.ToString())
                                    {
                                        currentCandle = candle.Value;
                                    }

                                }

                                // Ensure there is a candle for the current ticker
                                if (currentCandle.Minute == DateTime.MinValue)
                                {
                                    // OBTENER AQUÍ EL PRIMER PRECIO RECIBIDO PARA ESE MINUTO
                                    float firstPriceForMinute = 0;
                                    var pricesForTicker = priceDataList
                                    .Where(price => price.Id == ticker && price.TimeDT.Minute == currentMinute.Minute) //ticker
                                    .OrderBy(price => price.TimeDT)
                                    .ToList();

                                    // Asegurarse de que haya al menos un precio para ese minuto
                                    if (pricesForTicker.Count > 0)
                                    {
                                        // Devolver el precio más antiguo registrado para el minuto actual
                                        firstPriceForMinute = pricesForTicker[0].Price;
                                    }

                                    // Si no hay precios para ese minuto, puedes devolver un valor predeterminado o lanzar una excepción, según tus necesidades.
                                    // Aquí, simplemente devolvemos 0 como valor predeterminado, pero puedes ajustarlo a tu caso particular.


                                    if (firstPriceForMinute != 0)
                                    {
                                        var move = "";


                                        // OBTENER AQUÍ EL CANDLE ANTERIOR (puedes omitir esto si no lo necesitas)
                                        if (candlesData.TryGetValue(ticker, out Candlestick previousCandle))
                                        {
                                            //GUARDAR EN BASE DE DATOS LA VELA CERRADA 


                                            // Aquí puedes usar "previousCandle" que es el valor encontrado en el diccionario.
                                            // Si el valor se encuentra, el bloque dentro del "if" se ejecutará.
                                            // Si no se encuentra (es decir, el ticker no está en el diccionario), "previousCandle" será nulo y el bloque dentro del "if" no se ejecutará.


                                            if (firstPriceForMinute > previousCandle?.ClosePrice)
                                            {
                                                move = "🟢";
                                            }
                                            else if (firstPriceForMinute < previousCandle?.ClosePrice)
                                            {
                                                move = "🔴";
                                            }
                                            else if (firstPriceForMinute == previousCandle?.ClosePrice)
                                            {
                                                move = " = ";
                                            }
                                        }
                                        else
                                        {
                                            // Aquí puedes manejar el caso en el que el ticker no esté presente en el diccionario.
                                            // Por ejemplo, puedes crear un nuevo objeto Candlestick para ese ticker si lo deseas.
                                        }


                                        // Create a new candlestick for the current ticker
                                        currentCandle = new Candlestick
                                        {
                                            Minute = currentMinute,
                                            OpenPrice = firstPriceForMinute,
                                            ClosePrice = firstPriceForMinute,
                                            MaxPrice = firstPriceForMinute,
                                            MinPrice = firstPriceForMinute,
                                            CurrentPrice = currentPriceData.Price,
                                            Move = move
                                        };
                                    }
                                    candlesData[ticker] = currentCandle;

                                }
                                else
                                {
                                    // Update the existing candlestick for the current ticker
                                    if (currentCandle.CurrentPrice != currentPriceData.Price)
                                    {
                                        var move = "";
                                        if (currentCandle.CurrentPrice < currentPriceData.Price)
                                        {
                                            move = "🟢";
                                        }
                                        else if (currentCandle.CurrentPrice > currentPriceData.Price)
                                        {
                                            move = "🔴";
                                        }
                                        else if (currentCandle.CurrentPrice == currentPriceData.Price)
                                        {
                                            move = " = ";
                                        }

                                        currentCandle.Move = move;
                                    }

                                    currentCandle.CurrentPrice = currentPriceData.Price;
                                    currentCandle.ClosePrice = currentPriceData.Price;
                                    currentCandle.MaxPrice = Math.Max(currentCandle.MaxPrice, currentPriceData.Price);
                                    currentCandle.MinPrice = Math.Min(currentCandle.MinPrice, currentPriceData.Price);
                                }



                            }

                            _tickerDataMap[ticker] = new TickerData
                            {
                                CurrentPriceData = currentPriceData,
                                CurrentCandle = currentCandle,
                                DateTimeNow = DateTime.Now
                            };

                      
            }
        }
    }
}