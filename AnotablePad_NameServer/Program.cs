
/*
public class ProcessHandeler
{
    string lobbyServerProcessPath = "C:\\Users\\Lunox\\Source\\repos\\ProcessTester\\ProcessTester\\bin\\Release\\netcoreapp3.0\\ProcessTester.exe";
    string pipeName;
    int roomSerberId;

    public ProcessHandeler()
    {

    }

    public void runProcess()
    {

        pipeName = Utilities.GetRandomPassword(8);
        Console.WriteLine("Room Server Process Executing...");
        Process roomServer = new Process();
        roomServer.StartInfo.UseShellExecute = false;
        roomServer.StartInfo.FileName = lobbyServerProcessPath;
        roomServer.StartInfo.Arguments = pipeName;
        roomServer.StartInfo.CreateNoWindow = false;
        roomServer.Start();

        roomSerberId = roomServer.Id;

        NamedPipeServerStream pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut);

        // Wait for a client to connect
        Console.Write("Waiting for Room Server connection...");
        pipe.WaitForConnection();
        Console.WriteLine("Room Server connected.");

        try
        {
            StreamWriter sw = new StreamWriter(pipe);
            sw.AutoFlush = true;

            Thread.Sleep(100);
            Console.Write("Enter text: ");
            sw.WriteLine();



            StreamReader sr = new StreamReader(pipe);
            Console.WriteLine("Return : {0}", sr.ReadLine());
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
    }
}

class NameServer
{

    public static void OnEventHandling(NetEventState state)
    {

    }



    public static void Main(string[] args)
    {

        //string ProcessPath = "Tester\\ProcessTester.exe";



        TcpListener tcpListener = null;
        byte[] buffer = new byte[128];

        Socket host = null;
        Socket tablet = null;
        try
        {

            tcpListener = new TcpListener(IPAddress.Any, 4444);

            tcpListener.Start();

            Console.WriteLine("MuliThread Starting : Waiting for connections...");


            ProcessHandeler ph = new ProcessHandeler();

            Thread clientThread = new Thread(new ThreadStart(ph.runProcess));

            clientThread.Start();


        }
        catch (Exception exp)
        {
            Console.WriteLine("Exception :" + exp);
        }
        finally
        {
            tcpListener.Stop();
        }
    }

}
*/

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

public class ProcessHandeler
{
    string lobbyServerProcessPath = "C:\\Users\\Lunox\\Source\\repos\\ProcessTester\\ProcessTester\\bin\\Release\\netcoreapp3.0\\ProcessTester.exe";
    string pipeName;
    string roomServerPort;

    Socket host;
    Socket tablet;

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
    }
    public void runProcess()
    {

        byte[] buffer = new byte[128];

        pipeName = Utilities.GetRandomPassword(8);
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
        Console.Write("Waiting for Room Server connection...");
        pipeServer.WaitForConnection();
        Console.WriteLine("Room Server {0} connected.", threadId);
        try
        {
            StreamString ss = new StreamString(pipeServer);

            ss.WriteString("I am the one true server!"); //<<1

            //룸서버에 포트 전송
            roomServerPort = "5000";//Utilities.FindFreePort().ToString();
            ss.WriteString(roomServerPort); //<<3
            Thread.Sleep(100);

            //클라이언트에게 룸서버 포트 전송.
            buffer = Encoding.UTF8.GetBytes(roomServerPort);

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
    }
}


public class LobbyServer
{

    public static void Main()
    {

        TcpListener tcpListener = null;
        byte[] buffer = new byte[128];

        Socket host = null;
        Socket tablet = null;
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 4444);

            tcpListener.Start();

            Console.WriteLine("MuliThread Starting : Waiting for connections...");

            while (true)
            {
                Socket temp = tcpListener.AcceptSocket();

                int recvSize = temp.Receive(buffer, buffer.Length, SocketFlags.None);

                if (recvSize > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
                    string[] toks = message.Split("|");
                    foreach (var tok in toks)
                    {
                        if (tok == "") continue;
                        if (tok.Contains("@"))
                        {
                            if (tok == "@Host-PC")
                            {
                                host = temp;
                                Console.WriteLine("Host PC Connection");
                            }
                            else if (tok == "@Tablet")
                            {
                                tablet = temp;
                                Console.WriteLine("Tablet Connection");
                            }
                        }
                    }
                }
                else
                {
                    buffer = System.Text.Encoding.UTF8.GetBytes("@FAIL");
                    temp.Send(buffer, buffer.Length, SocketFlags.None);
                    continue;
                }

                if (host == null || tablet == null)
                    continue;

                ProcessHandeler rHandler = new ProcessHandeler(host, tablet);
                Thread roomHandler = new Thread(new ThreadStart(rHandler.runProcess));
                roomHandler.Start();
                host = tablet = null;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine("Exception :" + exp);
        }
        finally
        {
            tcpListener.Stop();
        }
        /*
        int i;
        Thread[] servers = new Thread[numThreads];

        Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
        Console.WriteLine("Waiting for client connect...\n");

        for (i = 0; i < numThreads; i++)
        {
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }
        Thread.Sleep(250);

        while (i > 0)
        {
            for (int j = 0; j < numThreads; j++)
            {
                if (servers[j] != null)
                {
                    if (servers[j].Join(250))
                    {
                        Console.WriteLine("Server thread[{0}] finished.", servers[j].ManagedThreadId);
                        servers[j] = null;
                        i--;    // decrement the thread watch count
                    }
                }
            }
        }
        */
        Console.WriteLine("\nServer threads exhausted, exiting.");
    }

    private static void ServerThread(object data)
    {
        NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut);

        int threadId = Thread.CurrentThread.ManagedThreadId;

        // Wait for a client to connect
        pipeServer.WaitForConnection();

        Console.WriteLine("Client connected on thread[{0}].", threadId);
        try
        {
            // Read the request from the client. Once the client has
            // written to the pipe its security token will be available.

            StreamString ss = new StreamString(pipeServer);

            // Verify our identity to the connected client using a
            // string that the client anticipates.

            ss.WriteString("I am the one true server!");
            string filename = ss.ReadString();

            // Read in the contents of the file while impersonating the client.
            ReadFileToStream fileReader = new ReadFileToStream(ss, filename);

            // Display the name of the user we are impersonating.
            Console.WriteLine("Reading file: {0} on thread[{1}] as user: {2}.",
                filename, threadId, pipeServer.GetImpersonationUserName());
            pipeServer.RunAsClient(fileReader.Start);
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
        pipeServer.Close();
    }
}

// Defines the data protocol for reading and writing strings on our stream
public class StreamString
{
    private Stream ioStream;
    private UTF8Encoding streamEncoding;

    public StreamString(Stream ioStream)
    {
        this.ioStream = ioStream;
        streamEncoding = new UTF8Encoding();
    }

    public string ReadString()
    {
        int len = 0;

        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        byte[] inBuffer = new byte[len];
        ioStream.Read(inBuffer, 0, len);

        return streamEncoding.GetString(inBuffer);
    }

    public int WriteString(string outString)
    {
        byte[] outBuffer = streamEncoding.GetBytes(outString);
        int len = outBuffer.Length;
        if (len > UInt16.MaxValue)
        {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();

        return outBuffer.Length + 2;
    }

    public int WriteByte(byte[] outString)
    {
        byte[] outBuffer = outString;
        int len = outBuffer.Length;
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();
        return len;
    }

    public byte[] ReadByte()
    {
        int len = 0;
        len = ioStream.ReadByte();
        byte[] inBuffer = new byte[len];
        ioStream.Read(inBuffer, 0, len);
        return inBuffer;
    }
}

// Contains the method executed in the context of the impersonated user
public class ReadFileToStream
{
    private string fn;
    private StreamString ss;

    public ReadFileToStream(StreamString str, string filename)
    {
        fn = filename;
        ss = str;
    }

    public void Start()
    {
        string contents = File.ReadAllText(fn);
        ss.WriteString(contents);
    }
}