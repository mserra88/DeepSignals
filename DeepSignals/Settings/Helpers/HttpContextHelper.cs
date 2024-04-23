using DeepSignals.Models;

namespace DeepSignals.Settings.Helpers
{
    public static class HttpContextHelper
    {
        public static Session GetSession(IHttpContextAccessor IHttpContextAccessor)
        {
            HttpContext httpContext = IHttpContextAccessor.HttpContext;

            IQueryCollection queryParameters = httpContext.Request.Query;
            string path = httpContext.Request.Path;

            // Dividimos la ruta en segmentos
            string[] pathSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Accedemos a los segmentos de la ruta
            string segment1 = pathSegments.Length > 0 ? pathSegments[0] : null;
            string segment2 = pathSegments.Length > 1 ? pathSegments[1] : null;
            // ...

            //var (IpStartTime, PreviousIpStartTime, Ip, Host, PreviousIp) = GetIpAndTime(httpContext);
            //var Headers = GetHeaders(httpContext);

            return new Session
            {
                //IpStartTime = IpStartTime,
                //PreviousIpStartTime = PreviousIpStartTime,
                //Ip = Ip,
                //Host = Host,
                //PreviousIp = PreviousIp,
                //Headers = Headers,
                //Cookie = GetCookie(httpContext, Headers),
                StartTime = GetStartTime(httpContext),
                UserId = GetUserId(httpContext)
            };
        }

        private static string GetUserId(HttpContext _httpContext)
        {
            var sessionGuid = SessionHelper.GetSessionString(_httpContext, Strings.Session.USER_ID);

            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = Guid.NewGuid().ToString();
                SessionHelper.SetSessionString(_httpContext, Strings.Session.USER_ID, sessionGuid);
            }

            return sessionGuid;
        }

        private static string GetStartTime(HttpContext _httpContext)
        {
            var sessionStartTime = SessionHelper.GetSessionString(_httpContext, Strings.Session.START_TIME);
            if (string.IsNullOrEmpty(sessionStartTime))
            {
                sessionStartTime = DateTime.UtcNow.ToString();
                SessionHelper.SetSessionString(_httpContext, Strings.Session.START_TIME, sessionStartTime);
            }

            return sessionStartTime;
        }


    }
}


/*
            var SESSION_IP = HttpContextAccessor.HttpContext.Session.GetString("REMOTE_IP_ADDRESS");
            var SESSION_IP_IS_NULL = String.IsNullOrEmpty(SESSION_IP);
            var CURRENT_IP = getIp();

            if (SESSION_IP_IS_NULL || !SESSION_IP_IS_NULL && SESSION_IP != CURRENT_IP)
            {
                SESSION_IP = CURRENT_IP;
                HttpContextAccessor.HttpContext.Session.SetString("REMOTE_IP_ADDRESS", SESSION_IP);
            }

            var TempUserConnection = new UserConnection
            {
                RemoteIpAddress = SESSION_IP
            };
*/
/*      
        bool reset = false;

        if (firstRender)
        {
            TempUserConnection.Headers = getHeader();
            TempUserConnection.Guid = Guid.NewGuid();

            Context.Session.SetString("PREVIOUS_IP", TempUserConnection.RemoteIpAddress);

            firstRender = false;
        }
        else
        {
            TempUserConnection.Headers = BlazorAppContext.UserConnection.Headers;
            TempUserConnection.Guid = BlazorAppContext.UserConnection.Guid;

            var PreviousIpAddress = Context.Session.GetString("PREVIOUS_IP");
            if (TempUserConnection.RemoteIpAddress != PreviousIpAddress)
            {
                TempUserConnection.PreviousIpAddress = PreviousIpAddress;
                Context.Session.SetString("PREVIOUS_IP", TempUserConnection.RemoteIpAddress);

                reset = true;
            }
        }
*/



















/*
    var SESSION_IP = Context.Session.GetString("REMOTE_IP_ADDRESS");
    var CURRENT_IP = getIp();

    if (String.IsNullOrEmpty(SESSION_IP) || SESSION_IP != CURRENT_IP)
    {
        Context.Session.SetString("REMOTE_IP_ADDRESS", CURRENT_IP);
    }

    var TempUserConnection = new UserConnection
    {
        RemoteIpAddress = Context.Session.GetString("REMOTE_IP_ADDRESS")
    };
*/