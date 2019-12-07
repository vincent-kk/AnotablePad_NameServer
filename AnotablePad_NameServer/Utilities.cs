using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// 유용한 함수를 모아놓고 사용하는 부분
/// </summary>
class Utilities
{
    /// <summary>
    /// 랜덤한 문자열을 지정 길이만큼 생성. pipe name으로 사용
    /// </summary>
    public static string GetRandomPassword(int _totLen)
    {
        Random rand = new Random();
        string input = "abcdefghijklmnopqrstuvwxyz0123456789";
        var chars = Enumerable.Range(0, _totLen).Select(x => input[rand.Next(0, input.Length)]);
        return new string(chars.ToArray());
    }

    /// <summary>
    /// 사용가능한 포트를 검색, 반환
    /// </summary>
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


