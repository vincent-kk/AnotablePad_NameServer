using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

public class RoomServerElement
{
    private ProcessHandeler process;
    private Thread thread;
    private bool isRunnig;
    private string name;
    private string password;
    private string port;

    public RoomServerElement(ProcessHandeler process, Thread thread)
    {
        this.Process = process;
        this.Thread = thread;
        Port = Process.RoomServerPort;
        IsRunnig = true;
    }
    public ProcessHandeler Process { get => process; set => process = value; }
    public Thread Thread { get => thread; set => thread = value; }
    public bool IsRunnig { get => isRunnig; set => isRunnig = value; }
    public string Name { get => name; set => name = value; }
    public string Password { get => password; set => password = value; }
    public string Port { get => port; set => port = value; }
}

public class ClientElement
{
    private ClientHandler handler;
    private Thread thread;
    public ClientElement(ClientHandler handler, Thread thread)
    {
        this.Handler = handler;
        this.Thread = thread;
    }
    public ClientHandler Handler { get => handler; set => handler = value; }
    public Thread Thread { get => thread; set => thread = value; }
}

class ThreadObserver
{
    private Thread thread;
    private TcpListenerManager listener;
    private List<RoomServerElement> rooms;
    private List<ClientElement> clients = new List<ClientElement>();
    private readonly int timeScale = 1000;
    public Thread Thread { get => thread; set => thread = value; }
    public TcpListenerManager Listener { get => listener; set => listener = value; }
    public List<RoomServerElement> Rooms { get => rooms; set => rooms = value; }
    public List<ClientElement> Clients { get => clients; set => clients = value; }
    public ThreadObserver(List<RoomServerElement> rooms, List<ClientElement> clients)
    {
        Rooms = rooms;
        Clients = clients;
    }
    public void runObserving()
    {
        Console.WriteLine("Start Observing");
        while (true)
        {
            CleanRoom();
            CleanClient();
        }
    }

    private void CleanRoom()
    {
        if (Rooms.Count > 0)
        {
            int timeSlice = timeScale / Rooms.Count;
            foreach (var room in Rooms)
            {
                if (room.IsRunnig)
                {
                    if (room.Thread.Join(timeSlice))
                    {
                        Console.WriteLine("Room Thread Joined");
                        Rooms.Remove(room);
                        if (Rooms.Count == 0) break;
                    }
                }
            }
        }
        else
        {
            Thread.Sleep(timeScale);
        }
    }

    private void CleanClient()
    {
        if (Clients.Count > 0)
        {
            int timeSlice = timeScale / Clients.Count;
            foreach (var client in Clients)
            {
                if (client.Thread.Join(timeSlice))
                {
                    Console.WriteLine("Client Thread Joined");
                    Clients.Remove(client);
                    if (Clients.Count == 0) break;
                }
            }
        }
        else
        {
            Thread.Sleep(timeScale);
        }
    }
}
