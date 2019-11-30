using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

class Utilities
{
    public static string GetRandomPassword(int _totLen)
    {
        Random rand = new Random();
        string input = "abcdefghijklmnopqrstuvwxyz0123456789";
        var chars = Enumerable.Range(0, _totLen).Select(x => input[rand.Next(0, input.Length)]);
        return new string(chars.ToArray());
    }

    public static int FindFreePort()
    {
        int port = 0;
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 0);
            socket.Bind(localEP);
            localEP = (IPEndPoint)socket.LocalEndPoint;
            port = localEP.Port;
        }
        finally
        {
            socket.Close();
        }
        return port;
    }
}


