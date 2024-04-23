using DeepSignals.Hubs;
using DeepSignals.Models;
using DeepSignals.Services;
using DeepSignals.Settings.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace DeepSignals.Workers
{
    /// <summary>
    /// HubDisconnectionWorker
    /// </summary> 
    #region Worker
    public sealed class HubDisconnectionWorker : BackgroundService
    {
        private static ConcurrentDictionary<string, Dictionary<string, (string, List<(string, string)>)>> _connectedUsers;
        private readonly ILogger<HubDisconnectionWorker> _logger;
        private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ClientManagerService _userManagerService;
        private readonly string ClassName;

        public HubDisconnectionWorker(
            ILogger<HubDisconnectionWorker> logger,
            IHubContext<ChatHub, IChatHub> chatHubContext,
            IMemoryCache memoryCache,
            ClientManagerService userManagerService)
        {
            ClassName = WorkerHelper.GetClassName<HubDisconnectionWorker>();
            _logger = logger;

            UpdateWorkerInfo();

            _chatHubContext = chatHubContext;
            _memoryCache = memoryCache;
            _userManagerService = userManagerService;
            _connectedUsers = _userManagerService._ConnectedUsers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            UpdateWorkerInfo("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Process();
                await Task.Delay(1_000, stoppingToken);
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

        private void UpdateWorkerInfo(string? state = "") { } //=> //WorkerHelper.UpdateWorkerInfo<HubDisconnectionWorker>(_logger, Strings.Workers.HubDisconnection_log, ClassName + "." + state);

        private async Task Process()
        {
            try
            {
                //var processingTasks = new List<Task>();

                IDictionary _groupsDictionary = CacheHelper.TryGetValueOrCreateAsync(_memoryCache, GetUsers, "ONLINE", "", TimeSpan.FromHours(6)).Result;

                foreach (DictionaryEntry entry in _groupsDictionary)
                {
                    //AddProcessingTask(processingTasks, async () =>
                    //{
                        var groupName = (string)entry.Key;
                        var groupValue = (ConcurrentDictionary<string, HubConnectionContext>)entry.Value;

                        UpdateWorkerInfo("COMPROBANDO " + groupValue.Keys.Count() + " Conexiones");

                        await ProcessGroup(groupName, groupValue.Keys);
                    //});
                }

                //await Task.WhenAll(processingTasks);
            }
            catch (Exception ex)
            {
                UpdateWorkerInfo("Process" + " Exception: " + ex.ToString());
            }
        }

        private async Task<IDictionary> GetUsers(string value, CancellationToken cancellationToken)
        {
            return GetPrivateField(GetPrivateField(GetPrivateField(_chatHubContext.Groups, "_lifetimeManager"), "_groups"), "_groups") as IDictionary;
        }

        private static object GetPrivateField(object obj, string fieldName) => obj.GetType().GetRuntimeFields().Single(fi => fi.Name == fieldName).GetValue(obj); //obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);

        private async Task ProcessGroup(string groupName, IEnumerable<string> groupValue)
        {
            //var processingTasks = new List<Task>();

            foreach (var connectionEntry in groupValue)
            {
                var connectionId = connectionEntry;
                //var value = connectionEntry.Value;

                //AddProcessingTask(processingTasks, async () =>
                //{
                    try
                    {
                        await CheckClientConnections(connectionId);
                    }
                    catch (Exception ex)
                    {
                        await HandleConnectionFailure(groupName, connectionId);
                    }
                //});
            }

            //await Task.WhenAll(processingTasks);
        }

        private async Task CheckClientConnections(string connectionId)
        {
            UpdateWorkerInfo("COMPROBANDO CONEXION ..." + connectionId);

            await _chatHubContext.Clients.Client(connectionId).SetClientMessageFromServer(DateTime.UtcNow.ToString());

            var message = await _chatHubContext.Clients.Client(connectionId).GetClientMessageFromServer();

            UpdateWorkerInfo(" CONEXION ..." + connectionId + " Comprobada a las " + message);
        }

        private async Task HandleConnectionFailure(
            string groupName,
            string connectionId)
        {
            try
            {
                CacheHelper.Remove(_memoryCache, "ONLINE");

                await _chatHubContext.Groups.RemoveFromGroupAsync(connectionId, "ONLINE");

                UpdateWorkerInfo("DESCONECTANDO CLIENTE..." + $"GroupName: {groupName}, Connection ID: {connectionId}");

                var (tabId, username) = GetTabIDAndUsernameFromConnectionID(connectionId);

                if (!string.IsNullOrEmpty(tabId) && !string.IsNullOrEmpty(username))
                {
                    _userManagerService.RemoveConnectionFROMTAB(new Session { UserId = username , TabId = tabId, ConnectonId = connectionId });

                    UpdateWorkerInfo("DESCONECTANDO CLIENTE..." + $"Username: {username}, Connection ID: {tabId}, Value: {connectionId}");
                }
            }
            catch (Exception ex)
            {
                UpdateWorkerInfo("ERROR DESCONECTANDO CLIENTE " + connectionId + " " + ex.ToString());
            }
        }

        private (string, string) GetTabIDAndUsernameFromConnectionID(string connectionID)
        {
            // Buscar la conexión ID en la lista de ConnectedUsers
            var userEntry = _connectedUsers.FirstOrDefault(u => u.Value.Any(c => c.Value.Item1 == connectionID));
    
            if (userEntry.Value != null)
            {
                string username = userEntry.Key;
                string tabID = userEntry.Value.FirstOrDefault(c => c.Value.Item1 == connectionID).Key;

                return (tabID, username);
            }

            return (null, null);
        }

        private static void AddProcessingTask(List<Task> processingTasks, Func<Task> action) => processingTasks.Add(Task.Run(action));
    }
    #endregion
}



//await Task.WhenAll(_tickers.SelectMany(ticker => _connectedUsers.SelectMany(userEntry => userEntry.Value.SelectMany(tabEntry => tabEntry.Value.Item2.Where(item => item.Item1 == ticker), (tabEntry, tickerTuple) => { }))));

/*
Parallel.For(0, 10, async i =>
{
});
*/



//SI ESTO SE PROCESARA A CADA 10 MINUTOS, NO HARIA FALTA. SI SE PROCESA A CADA SEGUNDO SERIA "INTERESANE", PERO Y SI ENTRAN Y SALEN MUCHOS A LA VEZ ?...........
//al entrar al hub se setea un user connected = true;

//se lee aqui

//si true se obtiene lista de hub y poner a false la propiedad estatica.

//si no
//se busca en cache

//si no se tiene en cache
//se obtiene lista hub

//tambien se puede hacer otro evento on disconected para otras causas.

/* 
    private IDictionary GetGroupsDictionary()
    {
        IGroupManager groupManager = _chatHubContext.Groups;
        object lifetimeManager = GetPrivateField(groupManager, "_lifetimeManager");
        object groupsObject = GetPrivateField(lifetimeManager, "_groups");
        return GetPrivateField(groupsObject, "_groups") as IDictionary;
    }

    IGroupManager groupManager = _chatHubContext.Groups;
    object lifetimeManager = groupManager.GetType().GetRuntimeFields().Single(fi => fi.Name == "_lifetimeManager").GetValue(groupManager);
    object groupsObject = lifetimeManager.GetType().GetRuntimeFields().Single(fi => fi.Name == "_groups").GetValue(lifetimeManager);
    return groupsObject.GetType().GetRuntimeFields().Single(fi => fi.Name == "_groups").GetValue(groupsObject) as IDictionary;

    private IDictionary GetGroupsDictionary1()
    {
        IGroupManager groupManager = _strongChatHubContext.Groups;
        object lifetimeManager = groupManager.GetType().GetRuntimeFields().Single(fi => fi.Name == "_lifetimeManager").GetValue(groupManager);
        object groupsObject = lifetimeManager.GetType().GetRuntimeFields().Single(fi => fi.Name == "_groups").GetValue(lifetimeManager);
        IDictionary groupsDictionary = groupsObject.GetType().GetRuntimeFields().Single(fi => fi.Name == "_groups").GetValue(groupsObject) as IDictionary;

        return groupsDictionary;
    }
*/

/*
 
    IGroupManager groupManager = _chatHubContext.Groups;
    FieldInfo lifetimeManagerField = groupManager.GetType().GetField("_lifetimeManager", BindingFlags.Instance | BindingFlags.NonPublic);
    object lifetimeManager = lifetimeManagerField.GetValue(groupManager);
    FieldInfo groupsField = lifetimeManager.GetType().GetField("_groups", BindingFlags.Instance | BindingFlags.NonPublic);
    object groupsObject = groupsField.GetValue(lifetimeManager);
    FieldInfo groupsDictionaryField = groupsObject.GetType().GetField("_groups", BindingFlags.Instance | BindingFlags.NonPublic);
    IDictionary<string, ConcurrentDictionary<string, HubConnectionContext>> groupsDictionary = (IDictionary<string, ConcurrentDictionary<string, HubConnectionContext>>)groupsDictionaryField.GetValue(groupsObject);

    return groupsDictionary;
*/


///obtener la lista de conexiones de signalRhub.....
/*
var x = _strongChatHubContext.Groups;
var lifetimeManagerValue = x?.GetType().GetField("_lifetimeManager", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(x);
var groupsValue = lifetimeManagerValue?.GetType().GetField("_groups", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(lifetimeManagerValue);
var innerGroupsValue = groupsValue?.GetType().GetField("_groups", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(groupsValue);                    
//ConcurrentDictionary<string, HubConnectionContext> groupNames2 = (ConcurrentDictionary<string, HubConnectionContext>)groupsDictionary.Values;
//List<string> groupNames = groupsDictionary.Keys.Cast<string>().ToList();
*/

/*
List<string> groupNames = groupsDictionary.Keys.Cast<string>().ToList();
var userGroups = groupsDictionary.Values.Cast<ConcurrentDictionary<string, HubConnectionContext>>();

var test = userGroups.Count();

foreach (var name in userGroups)
{
    var tttt = name.Count();
    await _strongChatHubContext.Clients.All.OnlineUsers(tttt.ToString());
    foreach (var name2 in name)
    {
        var key = name2.Key;
        var value = name2.Value;
    }
}

foreach (var name in groupNames)
{
    var t = name;
}
*/
///obtener la lista de conexiones de signalRhub.....

/*
List<string> connectionIds = new List<string>(_userListProvider.GetConnectionIdList());

foreach (var connectionId in connectionIds)
{
    await _strongChatHubContext.Clients.Client(connectionId).Ping("ping", connectionId);

    try
    {
        //var pong = await _strongChatHubContext.Clients.Client(connectionId).Pong();
    }
    catch (Exception ex)
    {
        //await _strongChatHubContext.Clients.All.PingPongError("PONG: " + ex.ToString());
    }
    //await _strongChatHubContext.Clients.Client(connectionId).Ping(pong);


}
*/
/*             
    var removeConnection = false;
    try
    {
        var pong = await _strongChatHubContext.Clients.Client(connectionId).Pong();
        if (pong != "pong")
        { 
            removeConnection = true;
        }
    }
    catch (Exception ex)
    {
        removeConnection = true;
    }

    if (removeConnection)
    {
        //_userListProvider.RemoveConnection(connectionId);
    }
*/


/*
    var proxyType = x.GetType();
    var proxyField = proxyType.GetField("_proxy", BindingFlags.NonPublic | BindingFlags.Instance);
    var proxyValue = proxyField.GetValue(x);

    var proxyValueType = proxyValue.GetType();
    var lifetimeManagerField = proxyValueType.GetField("_lifetimeManager", BindingFlags.NonPublic | BindingFlags.Instance);
    var lifetimeManagerValue = lifetimeManagerField.GetValue(proxyValue);

    var lifetimeManagerType = lifetimeManagerValue.GetType();
    var groupsField = lifetimeManagerType.GetField("_groups", BindingFlags.NonPublic | BindingFlags.Instance);
    var groupsValue = groupsField.GetValue(lifetimeManagerValue);

    // Acceder al miembro "_groups" dentro de "_groups"
    var innerGroupsField = groupsValue.GetType().GetField("_groups", BindingFlags.NonPublic | BindingFlags.Instance);
    var innerGroupsValue = innerGroupsField.GetValue(groupsValue);
*/







/////////////////////////////
///



/*
private void CreateTextFile(object state)
{
    string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_IHostedService.txt";
    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
    File.WriteAllText(filePath, "Hello, world!");
}



protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        //CODEHERE

        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}
public override Task StartAsync(CancellationToken cancellationToken)
{
    _logger.LogInformation("Background service is starting.");
    //return Task.CompletedTask;
    //return base.StartAsync(cancellationToken);
}

public override async Task StopAsync(CancellationToken cancellationToken)
{
    //_resetEvent.Set();
    //_timer?.Change(Timeout.Infinite, 0);
    //_timer?.Dispose();
    //return Task.CompletedTask;
    await base.StopAsync(cancellationToken);
}

public override void Dispose()
{
    //_timer?.Dispose();
    base.Dispose();
}
*/

/*  //SERVICE BASE
public BackgroundService()
{
InitializeComponent();
}

protected override void OnStart(string[] args)
{
_timer = new Timer(60000); // Ejecutar cada 1 minuto
_timer.Elapsed += Timer_Elapsed;
_timer.Start();
}

protected override void OnStop()
{
_timer.Stop();
}

private void Timer_Elapsed(object sender, ElapsedEventArgs e)
{
string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_ServiceBase.txt";
string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
File.WriteAllText(filePath, "Hello, world!");
}
*/


////////////////////////




/*




namespace DeepSignals.Server.Controllers
{
    /// <summary>
    /// This call will be used to send the data after each second to the client
    /// </summary>
    public class TimeWatcher
    {
        private Action? Executor;
        private Timer? timer;
        // we need to auto-reset the event before the execution
        private AutoResetEvent? autoResetEvent;


        public DateTime WatcherStarted { get; set; }

        public bool IsWatcherStarted { get; set; }

        /// <summary>
        /// Method for the Timer Watcher
        /// This will be invoked when the Controller receives the request
        /// </summary>
        public void Watcher(Action execute)
        {
            int callBackDelayBeforeInvokeCallback = 5250;
            int timeIntervalBetweenInvokeCallback = 5500;
            Executor = execute;
            autoResetEvent = new AutoResetEvent(false);
            timer = new Timer((obj) =>
            {
                Executor();
                //if ((DateTime.Now - WatcherStarted).TotalSeconds > 60)
                //{
                //    IsWatcherStarted = false;
                //    timer.Dispose();
                //}
            }, autoResetEvent, callBackDelayBeforeInvokeCallback, timeIntervalBetweenInvokeCallback);

            WatcherStarted = DateTime.Now;
            IsWatcherStarted = true;
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {
        private IHubContext<SignalRHub> marketHub;
        private TimeWatcher watcher;
        //private static Timer _timer;
        private readonly ILogger<Test> _logger;
        private readonly SqlClient _sqlClient;

        public MarketController(ILogger<Test> logger, IHubContext<SignalRHub> mktHub, SqlClient sqlClient)
        {
            _logger = logger;
            marketHub = mktHub;
            _sqlClient = sqlClient;

            //_timer = new Timer(SendDataToClients, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }


        private bool isWatcherStarted = false;
        private readonly object watcherLock = new object();
        private AutoResetEvent autoResetEvent;

        [HttpGet]
        public IActionResult Get1()
        {
            lock (watcherLock)
            {
                if (!isWatcherStarted)
                {
                    autoResetEvent = new AutoResetEvent(false);
                    isWatcherStarted = true;
                    Task.Factory.StartNew(() =>
                    //Task.Run(() =>
                    {
                        while (isWatcherStarted)
                        {
                            var marketData = _sqlClient.GetMarketData("Tickers", _logger);


                            foreach (var connectionId in SignalRHub.UserConnections)
                            {
                                marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
                            }
                            Thread.Sleep(500);
                            autoResetEvent.WaitOne(500);

                            //if (marketData!=null) { isWatcherStarted = false; }
                        }
                    });
                }
            }
            Task.Delay(500);

            return Ok(new { Message = "Request Completed" });
        }

        /*nofunciona. averiguar para hacerlo funcionar de este modo.
        [HttpGet]
        public async Task Get()
        {
            autoResetEvent = new AutoResetEvent(false);

            // Iniciamos la operación asincrónica
            await Task.Run(() =>
            {
                var marketData = MarketDataProvider.GetMarketData();
                foreach (var connectionId in MarketHub.UserConnections)
                {
                    marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
                }

            });

            autoResetEvent.Set();


            // Esperamos a que se complete la operación o transcurra un tiempo determinado
            var completed = await Task.Run(() => autoResetEvent.WaitOne(TimeSpan.FromSeconds(5)));

            if (completed)
            {
                //Console.WriteLine("Operación completada con éxito");
                 Ok(new { Message = "Request Completed" });

            }
            else
            {
                 Ok(new { Message = "Request NOT Completed" });

                //Console.WriteLine("Se agotó el tiempo de espera");
            }
        }
        */



/*
        [HttpGet]
        public IActionResult Get()
        {


            //Task.Delay(2000);

            if (!watcher.IsWatcherStarted)
            {
                //watcher.Watcher(()=>marketHub.Clients.All.SendAsync("SendMarketStatusData",MarketDataProvider.GetMarketData()));

                watcher.Watcher(() =>
                {
                    var marketData = MarketDataProvider.GetMarketData();
                    foreach (var connectionId in MarketHub.UserConnections)
                    {
                        marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
                    }
                });

            }

            return Ok(new { Message = "Request Completed" });
        }
*/








/*TimeWatcher builder.Services.AddSingleton<TimeWatcher>();
 * 


    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {
        private IHubContext<MarketHub> marketHub;
        private TimeWatcher watcher;
        private static Timer _timer;

        public MarketController(IHubContext<MarketHub> mktHub, TimeWatcher watch)
        {
            marketHub = mktHub;
            watcher = watch;
            //_timer = new Timer(SendDataToClients, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }


        private bool isWatcherStarted = false;
        private readonly object watcherLock = new object();
        private AutoResetEvent autoResetEvent;

        [HttpGet]
        public IActionResult Get1()
        {
            lock (watcherLock)
            {
                if (!isWatcherStarted)
                {
                    autoResetEvent = new AutoResetEvent(false);
                    isWatcherStarted = true;
                    Task.Factory.StartNew(() =>
                    //Task.Run(() =>
                    {
                        while (isWatcherStarted)
                        {
                            var marketData = MarketDataProvider.GetMarketData();
                            foreach (var connectionId in MarketHub.UserConnections)
                            {
                                marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
                            }
                            Thread.Sleep(500);
                            autoResetEvent.WaitOne(500);

                            //if (marketData!=null) { isWatcherStarted = false; }
                        }
                    });
                }
            }
             Task.Delay(500);

            return Ok(new { Message = "Request Completed" });
        }
*/






/*nofunciona. averiguar para hacerlo funcionar de este modo.
[HttpGet]
public async Task Get()
{
    autoResetEvent = new AutoResetEvent(false);

    // Iniciamos la operación asincrónica
    await Task.Run(() =>
    {
        var marketData = MarketDataProvider.GetMarketData();
        foreach (var connectionId in MarketHub.UserConnections)
        {
            marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
        }

    });

    autoResetEvent.Set();


    // Esperamos a que se complete la operación o transcurra un tiempo determinado
    var completed = await Task.Run(() => autoResetEvent.WaitOne(TimeSpan.FromSeconds(5)));

    if (completed)
    {
        //Console.WriteLine("Operación completada con éxito");
         Ok(new { Message = "Request Completed" });

    }
    else
    {
         Ok(new { Message = "Request NOT Completed" });

        //Console.WriteLine("Se agotó el tiempo de espera");
    }
}
*/



/*
        [HttpGet]
        public IActionResult Get()
        {


            //Task.Delay(2000);

            if (!watcher.IsWatcherStarted)
            {
                //watcher.Watcher(()=>marketHub.Clients.All.SendAsync("SendMarketStatusData",MarketDataProvider.GetMarketData()));

                watcher.Watcher(() =>
                {
                    var marketData = MarketDataProvider.GetMarketData();
                    foreach (var connectionId in MarketHub.UserConnections)
                    {
                        marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
                    }
                });

            }

            return Ok(new { Message = "Request Completed" });
        }
*/

/*

             var random = new Random();
             var marketData = new List<Market>()
             {
                 new Market() { CompanyName = "MS-IT Services", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "TS-IT Providers", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "LS-SL Sales", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "MS-Electronics", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "TS-Electrical", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "LS-Foods", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "MS-Healthcare", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "LS-Pharmas", Volume = random.Next(1,900)},
                 new Market() { CompanyName = "TS-Healthcare", Volume = random.Next(1,900)}
             };
             return marketData;
 */




/*

Si lo que deseas es solo recibir la primera respuesta del servidor en el método Get, una opción es modificar el método MarketDataProvider.GetMarketData() para que solo devuelva los datos una vez y después retorne null.De esta manera, en el método Get solo se recibiría la primera respuesta y no habría un bucle infinito.

Por ejemplo, podrías modificar el método MarketDataProvider.GetMarketData() de esta manera:


public static MarketData GetMarketData()
{
    if (IsMarketOpen)
    {
        return new MarketData()
        {
            MarketStatus = GetMarketStatus(),
            StockPrices = GetStockPrices()
        };

    }
    else
    {
        return null;
    }
}

*/



/*
Luego, en el método Get, puedes agregar una verificación adicional para asegurarte de que el MarketData que recibes no sea null, lo que significaría que MarketDataProvider.GetMarketData() ya no tiene más datos para enviar:

public IActionResult Get()
{
    if (!IsWatcherStarted)
    {
        autoResetEvent = new AutoResetEvent(false);
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                var marketData = MarketDataProvider.GetMarketData();
                if (marketData == null)
                {
                    break;
                }
                foreach (var connectionId in MarketHub.UserConnections)
                {
                    marketHub.Clients.Client(connectionId.Value).SendAsync("SendMarketStatusData", marketData);
                }
                autoResetEvent.WaitOne(5500);
            }
            IsWatcherStarted = false;
        });
        IsWatcherStarted = true;
    }

    return Ok(new { Message = "Request Completed" });
}

*/


/*



    }
}










*/