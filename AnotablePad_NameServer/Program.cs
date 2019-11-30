
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;

public class LobbyServer
{
    public static void OnEventHandling(NetEventState state)
    {

    }

    public static void Main()
    {
        List<RoomServerElement> rooms = new List<RoomServerElement>();
        List<ClientElement> clients = new List<ClientElement>();

        TcpListener tcpListener = null;
        byte[] buffer = new byte[128];

        Socket host = null;
        Socket tablet = null;

        ThreadObserver observer = new ThreadObserver(rooms, clients);
        Thread observerThread = new Thread(new ThreadStart(observer.runObserving));
        observerThread.Start();

        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 4444);

            tcpListener.Start();

            Console.WriteLine("MuliThread Starting : Waiting for connections...");

            while (true)
            {
                Socket temp = tcpListener.AcceptSocket();

                
                ClientHandler clientHandler = new ClientHandler(temp);
                Thread clientThread = new Thread(new ThreadStart(clientHandler.clientHandler));
                clients.Add(new ClientElement(clientHandler, clientThread));
                clientThread.Start();
                
                /*
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
                    buffer = Encoding.UTF8.GetBytes("@FAIL");
                    temp.Send(buffer, buffer.Length, SocketFlags.None);
                    continue;
                }

                if (host == null || tablet == null)
                    continue;

                ProcessHandeler roomProcess = new ProcessHandeler(host, tablet);
                Thread roomHandler = new Thread(new ThreadStart(roomProcess.runProcess));
                rooms.Add(new RoomServerElement(roomProcess, roomHandler));
                roomHandler.Start();
                host = tablet = null;
                */
                
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine("Exception :" + exp);
        }
        finally
        {
            tcpListener.Stop();
            observerThread.Join();
        }

        Console.WriteLine("AnotablePad NameServer is Closed...");
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
            Console.WriteLine("Reading file: {0} on thread[{1}] as user: {2}.", filename, threadId, pipeServer.GetImpersonationUserName());
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


public class ClientHandler
{
    Socket sock;
    bool isTablet;
    byte[] buffer;
    public ClientHandler(Socket sock)
    {
        this.Sock = sock;
        this.buffer = new byte[1024];
    }
    public Socket Sock { get => sock; set => sock = value; }
    public bool IsTablet { get => isTablet; set => isTablet = value; }

    public void clientHandler()
    {
        try
        {
            int recvSize = Sock.Receive(buffer, buffer.Length, SocketFlags.None);
            if (recvSize > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
                if (message == "@Tablet|") IsTablet = true;
                else IsTablet = false;
            }

            buffer = Encoding.UTF8.GetBytes("@CONNECTION");
            Sock.Send(buffer, buffer.Length, SocketFlags.None);
            while (true)
            {
                recvSize = Sock.Receive(buffer, buffer.Length, SocketFlags.None);
                if (recvSize > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
                    string returnMessage = IsTablet ? TabletRequest(message) : ComputerRequest(message);
                    buffer = Encoding.UTF8.GetBytes(returnMessage);
                    Sock.Send(buffer, buffer.Length, SocketFlags.None);
                }
                else
                {
                    buffer = Encoding.UTF8.GetBytes("@ERROR");
                    Sock.Send(buffer, buffer.Length, SocketFlags.None);
                    break;
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
    }
    private string TabletRequest(string request)
    {
        string msg;
        switch (request)
        {
            case "@FIND-ROOM|":
                msg = "@ROOM-LIST%AAA%BBB%CCC";
                break;
            case "@CREATE-ROOM|":
                msg = "@ROOMNAME";
                break;
            case "@ENTER-ROOM|":
                msg = "@ROOMNUMBER";
                break;
            default:
                msg = "@INVALIED";
                break;
        }
        return msg;
    }
    private string ComputerRequest(string request)
    {
        string msg;
        switch (request)
        {
            case "@FIND-ROOM|":
                msg = "@ROOM-LIST%AWEAD%GWEGSDf%QASDWEG%AWAWDA%QWEASD";
                break;
            case "@CREATE-ROOM|":
                msg = "@ROOMNAME";
                break;
            case "@ENTER-ROOM|":
                msg = "@ROOMNUMBER";
                break;
            default:
                msg = "@INVALIED";
                break;
        }
        return msg;
    }
}
