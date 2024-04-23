using DeepSignals.Models;
using DeepSignals.Settings.Helpers;
using DeepSignals.Settings.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeepSignals.Services
{
    public class SignalRService : IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public HubConnection? HubConnection { get; set; }

        //private List<string> _hubConnectionStateMessages = new List<string>();
        //public IReadOnlyList<string> HubConnectionStateMessages => _hubConnectionStateMessages.AsReadOnly();

        //public ConcurrentDictionary<string, Dictionary<string, (string, int)>> SIGNALRUSERS { get { return _clientManagerService.GetUsers(); } }

        public bool IsConnected => HubConnection?.State == HubConnectionState.Connected;
        private bool IsNotNull => HubConnection is not null;
        private bool IsDisconnected => HubConnection?.State == HubConnectionState.Disconnected;
        private bool NotDisconnected => HubConnection?.State != HubConnectionState.Disconnected;

        private Lazy<HubConnection>? LazyhubConnection;
        public ClientManagerService _clientManagerService { get; set; }
        public TickerDataService _tickerDataService { get; set; }

        public SignalRService(ClientManagerService clientManagerService, TickerDataService tickerDataService)
        {
            _clientManagerService = clientManagerService;
            _tickerDataService = tickerDataService;

            LazyhubConnection = new Lazy<HubConnection>(() =>
            {
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(new Uri(Strings.HubUrl), options =>  //Navigation.ToAbsoluteUri()
                    {
                        //options.Headers.Add("Authorization", $"Bearer {accessToken}");
                        options.Headers["test"] = "value";
//options.Proxy = WebClientHelper.Proxy();
                        options.HttpMessageHandlerFactory = innerHandler => new IncludeRequestCredentialsMessageHandler { InnerHandler = innerHandler };

                        //options.CloseTimeout = TimeSpan.FromSeconds(0);
                        //options.SkipNegotiation = true;
                        //options.Cookies.Add(new Cookie(/* ... */);
                        //options.ClientCertificates.Add(/* ... */);
                        //options.AccessTokenProvider = () => Task.FromResult(_myAccessToken);
                        //options.Transports = HttpTransportType.WebSockets;
                        //options.UseDefaultCredentials = true;
                    })
                    .AddMessagePackProtocol()
                    .WithAutomaticReconnect(new RandomRetryPolicy())
                    .Build();
                return hubConnection;
            });

            HubConnection = LazyhubConnection?.Value;

            //publicar event aggregator.? !
/*            
            HubConnection.Reconnecting += async (error) => AddHubConnectionStateMessage("Connection was lost and the client is reconnecting... " + error);
            HubConnection.Reconnected += async (error) => AddHubConnectionStateMessage("Connection was reestablished. " + error);
            HubConnection.Closed += async (error) =>
            {
                AddHubConnectionStateMessage("Connection has been closed or manually try to restart the connection. " + error);
                /*                                                                                                                                                         
                    //Dispose();                  
                    //AUTOMATIZAR RECONEXION.
                    await Task.Delay(1000); //await Task.Delay(new Random().Next(0, 5) * 1000);
                    await StartConnection();
                */
/*            };
*/
            //hubConnection.ServerTimeout = TimeSpan.FromSeconds(30);
            //hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(15);
            //hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(15);
            //

            /*
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Dispose(); // El usuario está cerrando la aplicación
            };
            */
        }



        private static async Task<bool> ConnectWithRetryAsync(HubConnection hubConnection, CancellationToken cancellationToken)
        {
            //Keep trying until we can start or the token is canceled.
            while (true)  //currentAttempt < maxAttempts && IsNotNull && IsDisconnected && !NotDisconnected && !IsConnected
            {
                try
                {
                    await hubConnection.StartAsync();
                    return true;
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        //public async Task AddSuscriber(Session session, string name, string type) => _clientManagerService.AddSuscriber(session, name, type);
        //public async Task RemoveSuscriber(Session session, string name, string type) => _clientManagerService.RemoveSubscriber(session, name, type);


        public async Task StartConnection(ProtectedStorageService ProtectedStorageService, Session session)
        {
            session.TabId = await ProtectedStorageService.GetTabSessionGUID();
            //session.currentVisits = await ProtectedStorageService.GetTabSessionVisits();

            CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token;

            if (await ConnectWithRetryAsync(HubConnection, cancellationToken))
            {
                session.ConnectonId = HubConnection.ConnectionId;

                _clientManagerService.AddConnection(session);

                await LastUpdate_INIT(ProtectedStorageService);
            }
            else
            {
            }
        }

        public virtual async ValueTask DisposeAsync(Session session)
        {
            if (IsNotNull && IsConnected)
            {
                _clientManagerService.RemoveConnectionFROMTAB(session);

                await HubConnection.DisposeAsync();
            }
        }

        #region LASTUPDATE COMPONENT
        private async Task LastUpdate_INIT(ProtectedStorageService ProtectedStorageService)
        {
            HubConnection?.On<string>(Strings.Events.SetClientMessageFromServer, async (message) =>
            {
                await ProtectedStorageService.SetLastUpdate(message);
            });

            //GetClientMessageFromServer
            HubConnection?.On<string>(Strings.Events.GetClientMessageFromServer, async () =>
            {
                var result = await ProtectedStorageService.GetLastUpdate();;
                return result.Value;
            });
        }
        #endregion
    }
}