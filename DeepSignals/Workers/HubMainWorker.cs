using DeepSignals.Hubs;
using DeepSignals.Models;
using DeepSignals.Services;
using DeepSignals.Settings.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Polly.Retry;
using Polly;
using System.Net.WebSockets;
using System.Text;

namespace DeepSignals.Workers
{
    /// <summary>
    /// HubMainWorker
    /// </summary> 
    #region Worker
    public sealed class HubMainWorker : BackgroundService
    {
        private readonly ILogger<HubMainWorker> _logger;
        private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
        private readonly SQLService _sqlService;
        private readonly TickerDataService _tickerDataService;
        private readonly string _class;

        public HubMainWorker(
            ILogger<HubMainWorker> logger,
            IHubContext<ChatHub, IChatHub> chatHubContext,
            SQLService sqlService,
            TickerDataService tickerDataService)
        {
            _class = WorkerHelper.GetClassName<HubMainWorker>();
            _logger = logger;
            UpdateWorkerInfo();

            _tickerDataService = tickerDataService;
            _chatHubContext = chatHubContext;
            _sqlService = sqlService;
        }

        private static AsyncRetryPolicy CreateRetryPolicy()
        {
            return Policy
                .Handle<WebSocketException>() // Manejar excepciones de WebSocket
                .WaitAndRetryAsync(
                    3, // Número de intentos de reintento
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Intento de reconexión ({retryCount}) tras una excepción: {exception}");
                    }
                );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ClientWebSocket _ws = new ClientWebSocket();

            try
            {
                UpdateWorkerInfo("ExecuteAsync");

                byte[] buffer = new byte[1024];

                var _tickers = _sqlService.Tickers.Select(item => item.name);
                //var _tickers = _sqlService.Types.SelectMany(group => group.Select(item => item.name));


await _ws.ConnectAsync(new Uri("wss://streamer.finance.yahoo.com:443"), CancellationToken.None);

                StringBuilder subscribe = new StringBuilder();
             
                subscribe.Append("{\"subscribe\":[");
                subscribe.Append(string.Join(",", _tickers.Select(ticker => $"\"{ticker}\"")));
                subscribe.Append("]}");
    
//subscribe.Append("{\"subscribe\":[\"^DJI\"]}");

await _ws.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(subscribe.ToString())), WebSocketMessageType.Text, true, CancellationToken.None);

                List<PriceData> priceDataList = new List<PriceData>();

                // Start a separate task to continuously read pricing data from the WebSocket
                _ = Task.Run(async () =>
                {
                    byte[] buffer = new byte[1024];

                    try
                    {
                        while (_ws.State == WebSocketState.Open)
                        {
                            WebSocketReceiveResult result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                      
                                priceDataList.Add(WebSocketHelper.DecodePricingData(Convert.FromBase64String(Encoding.UTF8.GetString(buffer, 0, result.Count))));


                                Console.WriteLine(DateTime.Now + priceDataList.Last().Id + priceDataList.Last().Price);


                                //OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.
                                _tickerDataService.SetTickerData(_tickers.ToList(), priceDataList);
                                //OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.//OJO QUITAR EL TOLIST.

//await _chatHubContext.Clients.All.StateHasChanged();

                            }
                        }
                    }
                    catch (WebSocketException ex)
                    {
                        // Handle exceptions in the WebSocket connection.
                        Console.WriteLine("WebSocket Exception: " + ex.Message);
                    }
                });


     


                while (!stoppingToken.IsCancellationRequested)
                {
                    //Console.WriteLine("whileee" + DateTime.Now);

                    /*
                                        if (_ws.State != WebSocketState.Open)
                                        {
                                            // Ensure WebSocket is closed when exiting
                                            /*
                                            if (_ws != null && _ws.State == WebSocketState.Open)
                                            {
                                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                                            }
                                            _ws.Dispose();

                                            await CreateRetryPolicy().ExecuteAsync(async () =>
                                            {
                                                Console.WriteLine("RECONNECTING: " + DateTime.Now);
                                            });
                                            */
                    /*                   }
                                       else
                                       {
                                           try
                                           {
                                               WebSocketReceiveResult result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                                               if (result.MessageType == WebSocketMessageType.Text)
                                               {
                                                   priceDataList.Add(WebSocketHelper.DecodePricingData(Convert.FromBase64String(Encoding.UTF8.GetString(buffer, 0, result.Count))));


                                                   Console.WriteLine("Dato recibido desde yahoo por lo que se actualiza la ui del usuario." + DateTime.Now + priceDataList.Last().Id + priceDataList.Last().Price);



                                                   _tickerDataService.SetTickerData(_tickers, priceDataList);


                                                   await _chatHubContext.Clients.All.StateHasChanged();
                                               }
                                               // Rest of your processing logic...
                                           }
                                           catch (WebSocketException ex)
                                           {
                                               // Log the exception details for debugging
                                               Console.WriteLine("WebSocketException: " + ex.ToString());
                                           }


                                       }
                   */
                    //Console.WriteLine("next" + DateTime.Now);
                    await Task.Delay(1_000, stoppingToken);
                }




            }
            catch (Exception ex)
            {
                UpdateWorkerInfo("Process" + " Exception: " + ex.ToString());
            }
            finally
            {
                // Ensure WebSocket is closed when exiting
                if (_ws != null && _ws.State == WebSocketState.Open)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                _ws.Dispose();
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            UpdateWorkerInfo("StopAsync");
            await base.StopAsync(stoppingToken);
        }

        public void Dispose()
        {
            UpdateWorkerInfo("Dispose");
            base.Dispose();
        }

        private void UpdateWorkerInfo(string? state = "") => WorkerHelper.UpdateWorkerInfo<HubMainWorker>(_logger, Strings.Workers.HubMainWorkerlog, _class + "." + state);
    }
}
#endregion