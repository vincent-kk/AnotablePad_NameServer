using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
class SocketManager
{
    private Socket socket;

    private PacketQueue sendQueue;

    private PacketQueue receiveQueue;

    protected bool dispatchThreadLoop = false;

    protected Thread dispatchThread = null;

    private Thread workerThread = null;

    private bool isConnected = false;

    private static int BUFFERSIZE = 1024;

    public delegate void EventHandler(NetEventState state);

    private EventHandler handler;

    public bool IsConnected { get => isConnected;}

    public SocketManager()
    {
        sendQueue = new PacketQueue();
        receiveQueue = new PacketQueue();
    }

    public bool StartSocket(Socket socket)
    {
        Console.WriteLine("Socket Connected");
        try
        {
            this.socket = socket;
            isConnected = true;
        }
        catch
        {
            Console.WriteLine("Socket Fail");
            return false;
        }

        return LaunchdispatchThread();
    }

    public int Send(byte[] data, int size)
    {
        if (sendQueue == null)
        {
            return 0;
        }
        return sendQueue.Enqueue(data, size);
    }

    public int Receive(ref byte[] buffer, int size)
    {
        if (receiveQueue == null)
        {
            return 0;
        }

        return receiveQueue.Dequeue(ref buffer, size);
    }


    public void Disconnect()
    {
        isConnected = false;

        if (socket != null)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }

        if (handler != null)
        {
            NetEventState state = new NetEventState();
            state.type = NetEventType.Disconnect;
            state.result = NetEventResult.Success;
            handler(state);
        }
    }


    private bool LaunchdispatchThread()
    {
        try
        {
            dispatchThreadLoop = true;
            dispatchThread = new Thread(new ThreadStart(Dispatch));
            dispatchThread.Start();
        }
        catch
        {
            Console.WriteLine("Cannot launch dispatchThread.");
            return false;
        }
        return true;
    }


    public void Dispatch()
    {
        Console.WriteLine("Dispatch dispatchThread started.");

        while (dispatchThreadLoop)
        {
            if (socket != null && isConnected == true)
            {
                DispatchReceive();
                DispatchSend();
            }
            Thread.Sleep(50);
        }
        Console.WriteLine("Dispatch dispatchThread ended.");
    }


    void DispatchSend()
    {
        try
        {
            if (socket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] buffer = new byte[BUFFERSIZE];

                int sendSize = sendQueue.Dequeue(ref buffer, buffer.Length);
                while (sendSize > 0)
                {
                    socket.Send(buffer, sendSize, SocketFlags.None);
                    sendSize = sendQueue.Dequeue(ref buffer, buffer.Length);
                }
            }
        }
        catch
        {
            return;
        }
    }

    void DispatchReceive()
    {
        try
        {
            while (socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[BUFFERSIZE];
                int recvSize = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                if (recvSize == 0) Disconnect();
                else if (recvSize > 0) receiveQueue.Enqueue(buffer, recvSize);
            }
        }
        catch
        {
            return;
        }
    }

    public void RegisterEventHandler(EventHandler handler)
    {
        this.handler += handler;
    }


    public void UnregisterEventHandler(EventHandler handler)
    {
        this.handler -= handler;
    }


    public Thread GetThread()
    {
        return workerThread;
    }

    public void SetThread(Thread thread)
    {
        workerThread = thread;
    }

}

public class TcpListenerManager
{
    private bool isListening;
    private TcpListener tcpListener;
    public bool IsListening { get => isListening; set => isListening = value; }
    public TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }
    public TcpListenerManager(string port)
    {
        TcpListener = new TcpListener(IPAddress.Any, Int32.Parse(port));
        IsListening = true;
    }
}