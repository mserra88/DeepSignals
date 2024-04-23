using DeepSignals.Hubs;
using DeepSignals.Models;
using System.ComponentModel;
using System;
using System.Net;
using System.Reflection;

public static class StringExtensions
{
    public static string ReplaceSpecialCharacters(this string value) => value.Replace("%5E", "^").Replace("%23", "#"); // Agrega más reemplazos según tus necesidades
}



public static class Strings
{
    public static string AppName => "DeepSignals";

    public static string HubEndPoint => "/chathub";

    public static string HubUrl => Host + HubEndPoint;

    public static string Host => "https://" + ((Dns.GetHostName().ToString() == "DESKTOP-CATEEQS") ? "localhost:7123" : "hemeroskopeion.com");

    public static class Events
    {
        public static string GetClientMessageFromServer => nameof(IChatHub.GetClientMessageFromServer);
        public static string SendPublicMessage => nameof(ChatHub.SendPublicMessage);
        public static string SetClientMessageFromServer => nameof(IChatHub.SetClientMessageFromServer);

        public static string StateHasChanged => nameof(IChatHub.StateHasChanged);
        
        public static string ShowPublicMessage => nameof(IChatHub.ShowPublicMessage);
    }

    public static class Workers
    {
        public static string HubDisconnection_log = "HubDisconnectionWorker_log.txt";
        public static string HubMainWorkerlog = "HubMainWorker_log.txt";
    }

    public static class Proxy
    {
        public static string Alternative_1 => "http://winproxyus1.1and1.com:3128";
        public static string Alternative_2 => "http://ntproxy.1and1.com:3128";
        public static string Default => "http://winproxy.server.lan:3128";
    }

    public static class Session
    {
        public const string IP = "IP";
        public const string IP_START_TIME = "IP_START_TIME";
        public const string PREVIOUS_IP = "PREVIOUS_IP";
        public const string PREVIOUS_IP_START_TIME = "PREVIOUS_IP_START_TIME";
        public const string START_TIME = "START_TIME";
        public const string USER_ID = "USER_ID";
    }

    public static class ShowHistorical
    {
        public static class Type
        {
            public const string Chart = "Chart";
            public const string Detail = "Detail";
            public const string Historical = "Historical";
        }
    }

    public static class Ticker
    {
        public static class Source
        {
        }

        public static class Type
        {
        }
    }
}