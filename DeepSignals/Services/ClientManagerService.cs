using DeepSignals.Models;
using DeepSignals.Settings.Helpers;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DeepSignals.Services
{
    public class ClientManagerService
    {
        private IMemoryCache _memoryCache;

        private static readonly ConcurrentDictionary<string, Dictionary<string, (string, List<(string, string)>)>> ConnectedUsers = new ConcurrentDictionary<string, Dictionary<string, (string, List<(string, string)>)>>();

        public ClientManagerService(IMemoryCache memoryCache)
        { 
            _memoryCache= memoryCache;
        }

        public ConcurrentDictionary<string, Dictionary<string, (string, List<(string, string)>)>> _ConnectedUsers
        {
            get
            {
                lock (ConnectedUsers)
                {
                    return ConnectedUsers;
                }
            }
        }

        public string _ConnectedUsersCount
        {
            get
            {
                lock (_ConnectedUsers)
                {
                    return _ConnectedUsers.Count().ToString();
                }
            }
        }

        public void AddConnection(Session session)
        {
            string userid = session.UserId;
            string tabId = session.TabId;
            string connectionId = session.ConnectonId;

            lock (ConnectedUsers)
            {
                CacheHelper.Remove(_memoryCache, "ONLINE");

                if (string.IsNullOrEmpty(userid))
                {
                    Trace.TraceInformation("User not logged in, can't connect to SignalR service");
                    return;
                }

                Trace.TraceInformation(userid + " connected");

                //incrementVisits();

                // Save connection
                Dictionary<string, (string, List<(string, string)>)> userConnections;
                if (!ConnectedUsers.TryGetValue(userid, out userConnections))
                {
                    userConnections = new Dictionary<string, (string, List<(string, string)>)>();
                    ConnectedUsers.TryAdd(userid, userConnections);
                }

                if (userConnections.ContainsKey(tabId))
                {

                    var connectionInfo = userConnections[tabId];
                    connectionInfo.Item1 = connectionId; // Update existing connectionId for the tabId

                    userConnections[tabId] = connectionInfo;
                }
                else
                {
                    userConnections.Add(tabId, (connectionId, new List<(string, string)>())); // Add new connectionId for the tabId with initial visit count of 0

                }
            }
        }

        public void RemoveConnectionFROMTAB(Session session)
        {
            string userid = session.UserId;
            string tabId = session.TabId;
            string connectionId = session.ConnectonId;


            lock (ConnectedUsers)
            {
                CacheHelper.Remove(_memoryCache, "ONLINE");

                if (ConnectedUsers.TryGetValue(userid, out var userConnections))
                {
                    if (userConnections.ContainsKey(tabId))
                    {
                        var connectionInfo = userConnections[tabId];
                        if (connectionInfo.Item1 == connectionId)
                        {
                            userConnections.Remove(tabId);

                            if (userConnections.Count == 0)
                            {
                                ConnectedUsers.TryRemove(userid, out _);
                            }
                        }
                    }
                }
            }

        }
    }
}

/*
public int Visits=> visits;

private static int visits = 0;
private void incrementVisits() => visits++;
*/

/*
public interface ISignalRConnections
{
    void AddConnection(string connectionId, string userId);
    void RemoveConnection(string connectionId);
    string GetUserId(string connectionId);
}
 : ISignalRConnections
*/


//UserHandler.ConnectedIds.Add(Context.ConnectionId);

//RemoveUserConnection(Context);
//UserHandler.ConnectedIds.Remove(Context.ConnectionId);
/* 
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }
*/





/*
public IReadOnlyDictionary<string, string> UserConnections()
{
    return ConnectedUsers;
}

public int UserConnectionsCount()
{
    return UserConnections().Count();
}

*/



/*
public async Task AddSuscriber(Session session, string name, string type)
{
    var UserId = session.UserId;
    var TabId = session.TabId;

    // Verificar si el UserId y el TabId existen en ConnectedUsers
    if (ConnectedUsers.TryGetValue(UserId, out var userDictionary) &&
        userDictionary.TryGetValue(TabId, out var userTuple))
    {
        // Verificar si el valor name no existe en la lista de strings
        if (!userTuple.Item2.Contains((name, type)))
        {
            // Agregar el nuevo valor a la lista de strings
            userTuple.Item2.Add((name, type));
        }
    }
}

public async Task RemoveSubscriber(Session session, string name, string type)
{
    var UserId = session.UserId;
    var TabId = session.TabId;

    // Verificar si el UserId y el TabId existen en ConnectedUsers
    if (ConnectedUsers.TryGetValue(UserId, out var userDictionary) &&
        userDictionary.TryGetValue(TabId, out var userTuple))
    {
        // Verificar si el valor name existe en la lista de strings
        if (userTuple.Item2.Contains((name, type)))
        {
            // Remover el valor de la lista de strings
            userTuple.Item2.Remove((name, type));
        }
    }
}
*/


/*
public void RemoveConnectionTAB(Session session)
{
    string userid = session.UserId;
    string tabId = session.TabId;

    lock (ConnectedUsers)
    {
        Dictionary<string, (string, List<(string, string)>)> userConnections;
        if (ConnectedUsers.TryGetValue(userid, out userConnections))
        {
            if (userConnections.Remove(tabId))
            {
                if (userConnections.Count == 0)
                {
                    Dictionary<string, (string, List<(string, string)>)> garbage;
                    ConnectedUsers.TryRemove(userid, out garbage);
                }
            }
        }
    }
}
*/



/*
 * 
 * 
 *     public class UserListProvider
{
    private static List<string> ConnectionIdList = new List<string>();
    private static readonly object collectionLock = new object();

    public List<string> GetConnectionIdList()
    {
        lock (collectionLock)
        {
            return ConnectionIdList;
        }
    }

    public void AddConnection(string connectionId)
    {
        lock (collectionLock)
        {
            ConnectionIdList.Add(connectionId);
        }
    }

    public void RemoveConnection(string connectionId)
    {
        lock (collectionLock)
        {
            ConnectionIdList.Remove(connectionId);
        }
    }
}*/

/*
        private static ConcurrentDictionary<string, List<string>> ConnectedUsers = new ConcurrentDictionary<string, List<string>>();

        public ConcurrentDictionary<string, List<string>> GetUsers()
        {
            lock (ConnectedUsers)
            {
                return ConnectedUsers;
            }
        }

            public void AddConnection(string userid, string tabId, string connectionId)
        {
            lock (ConnectedUsers)
            {


                    if (userid == null || userid.Equals(string.Empty))
                    {
                        Trace.TraceInformation("user not loged in, can't connect signalr service");
                        return;
                    }
                    Trace.TraceInformation(userid + "connected");
                    // save connection
                    List<string>? existUserConnectionIds;
                    ConnectedUsers.TryGetValue(userid, out existUserConnectionIds);
                    if (existUserConnectionIds == null)
                    {
                        existUserConnectionIds = new List<string>();
                    }
                    existUserConnectionIds.Add(connectionId);
                    ConnectedUsers.TryAdd(userid, existUserConnectionIds);
            }
        }

        public void RemoveConnection(string userid, string tabId, string connectionId)
        {
            lock (ConnectedUsers)
            {

                //contador de pestañas. con pbs
                //se suma el guid , al sesionprotected guid, tenemos, pesetañas.

                List<string>? existUserConnectionIds;
            ConnectedUsers.TryGetValue(userid, out existUserConnectionIds);

            //se podria hacer que se le pase el tab id , borrar todos los cid de ese tabid y punto.
            existUserConnectionIds.Remove(connectionId);

            if (existUserConnectionIds.Count == 0)
            {
                List<string> garbage;
                ConnectedUsers.TryRemove(userid, out garbage);
            }


                /*
                ConnectedUsers.TryGetValue(userid, out existUserConnectionIds);
                if (existUserConnectionIds == null)
                {
                    existUserConnectionIds = new List<string>();
                }
                existUserConnectionIds.Add(Context.ConnectionId);
                ConnectedUsers.TryAdd(userid, existUserConnectionIds);



                if (existUserConnectionIds.Count == 0)
                            {
                                List<string> garbage;
                                ConnectedUsers.TryRemove(userid, out garbage);
                            */

/*

            }

        }
*/

