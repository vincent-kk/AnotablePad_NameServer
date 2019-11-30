using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ProcessHandeler
{
    private string lobbyServerProcessPath = "C:\\Users\\Lunox\\Source\\repos\\ProcessTester\\ProcessTester\\bin\\Release\\netcoreapp3.0\\AnotablePad_RoomServer.exe";
    private string pipeName;
    private string roomServerPort;

    private Socket host;
    private Socket tablet;

    public ProcessHandeler()
    {
        Console.WriteLine("Room Handling Thread is Running");
    }

    public ProcessHandeler(Socket host)
    {
        this.host = host;
    }

    public ProcessHandeler(Socket host, Socket tablet)
    {
        this.host = host;
        this.tablet = tablet;

        pipeName = Utilities.GetRandomPassword(8);
        RoomServerPort = "5000";//Utilities.FindFreePort().ToString();
    }

    public string RoomServerPort { get => roomServerPort; set => roomServerPort = value; }

    public void runProcess()
    {

        byte[] buffer = new byte[128];
        Console.WriteLine("Room Server Process Executing...");
        Process roomServer = new Process();
        roomServer.StartInfo.UseShellExecute = false;
        roomServer.StartInfo.FileName = lobbyServerProcessPath;
        roomServer.StartInfo.Arguments = pipeName;
        roomServer.StartInfo.CreateNoWindow = false;
        roomServer.Start();

        NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);

        int threadId = Thread.CurrentThread.ManagedThreadId;

        // Wait for a client to connect
        Console.WriteLine("Waiting for Room Server connection...");
        pipeServer.WaitForConnection();
        Console.WriteLine("Room Server {0} connected.", threadId);
        try
        {
            StreamString ss = new StreamString(pipeServer);

            ss.WriteString("@NameServer:StartRoom"); //<<1

            //룸서버에 포트 전송
            ss.WriteString(RoomServerPort); //<<3
            Thread.Sleep(100);

            //클라이언트에게 룸서버 포트 전송.
            buffer = Encoding.UTF8.GetBytes(RoomServerPort);

            host.Send(buffer, buffer.Length, SocketFlags.None);
            tablet.Send(buffer, buffer.Length, SocketFlags.None);

            Thread.Sleep(100);
            var temp = ss.ReadString();
            Console.WriteLine(temp);
        }
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }

        roomServer.WaitForExit();

        pipeServer.Close();

        Console.WriteLine("Room Server Closed Dictected");
    }
}