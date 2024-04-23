using DeepSignals.Models;
using DeepSignals.Settings.Helpers;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using static DeepSignals.Workers.HubMainWorker;

namespace DeepSignals.Hubs;

#region IChatHub
public interface IChatHub
{
    Task<string> GetClientMessageFromServer();    
    Task ShowPublicMessage(string user, string message);
    Task SetClientMessageFromServer(string message);

    Task StateHasChanged();
}
#endregion

#region ChatHub
public class ChatHub : Hub<IChatHub>
{
    private readonly ILogger<ChatHub> _logger;
    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
//var test = Context.GetHttpContext().Request.Headers["test"].ToString();

await Groups.AddToGroupAsync(Context.ConnectionId, "ONLINE");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        //RemoveFromGroupAsync does not need to be called in OnDisconnectedAsync, it's automatically handled for you.

await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ONLINE");//EN UN PRINCIPIO NO ES NECESARIO PERO MEJOR ASI. ASI AYUDA A QUE PROCESE MENOS.

        await base.OnDisconnectedAsync(exception);
    }

    //public async Task<string> WaitForMessage(string connectionId) => await Clients.Client(connectionId).GetClientMessageFromServer();

    public async Task SendPublicMessage(string user, string message)//, IDatabaseService dbService
    {
        try
        {
                        //var x = Context.GetHttpContext().Request.Headers["GUID"].ToString();
                        if (!string.IsNullOrEmpty(user))
                        {
                            user = " as " + user;
                        }
                        var x = Context.ConnectionId + user;

            await SendMessageToAll(x, message);




                        // Obtener los valores del contexto del servidor
                        var ipAddress = Context.GetHttpContext().Connection.RemoteIpAddress?.ToString();
                        var port = Context.GetHttpContext().Connection.RemotePort.ToString();
                        var userIdentity = Context.GetHttpContext().User.Identity?.Name;
                        var connectionId = Context.ConnectionId;
                        var userAgent = Context.GetHttpContext().Request.Headers["User-Agent"].ToString();
                        var requestMethod = Context.GetHttpContext().Request.Method;
                        var requestPath = Context.GetHttpContext().Request.Path;
                        var isHttps = Context.GetHttpContext().Request.IsHttps;
                        var protocol = Context.GetHttpContext().Request.Protocol;
                        var host = Context.GetHttpContext().Request.Host;

                        // Crear un array con los nombres y los valores
                        var values = new[]
                        {
                            $"IpAddress: {ipAddress}",
                            $"Port: {port}",
                            $"UserIdentity: {userIdentity}",
                            $"ConnectionId: {connectionId}",
                            $"UserAgent: {userAgent}",
                            $"RequestMethod: {requestMethod}",
                            $"RequestPath: {requestPath}",
                            $"IsHttps: {isHttps}",
                            $"Protocol: {protocol}",
                            $"Host: {host}"
                        };

                        // Combinar los valores en un solo string
                        var mergedString = string.Join(" | ", values);

            await SendMessageToCaller(mergedString, message);
        }
        catch (Exception ex)
        {

            //
            //hacer uno para errores de hub especifico.
            //await Clients.Caller.ShowBackgroundServiceStatusMessage(ex.Message);
            //            UpdateWorkerInfo("Exception in Process " + ex.ToString());
            WorkerHelper.UpdateWorkerInfo<ChatHub>(_logger, "ChatHub.txt", "ChatHub.SendPublicMessage Exception: " + ex.Message);


            throw new HubException("This error will be sent to the client!");
        }
    }

    public async Task SendMessageToAll(string user, string message)
        //=> await Clients.All.SendAsync("ReceiveMessage", user, message);
        => await Clients.All.ShowPublicMessage(user, message);

    public async Task SendMessageToCaller(string user, string message)
        //=> await Clients.Caller.SendAsync("ReceiveMessage", user, message);
        => await Clients.Caller.ShowPublicMessage(user, message);
    public async Task SendMessageToGroup(string user, string message)
        //=> await Clients.Group("ONLINE").SendAsync("ReceiveMessage", user, message);
        => await Clients.Group("ONLINE").ShowPublicMessage(user, message);

}
#endregion