using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace AnotablePad_NameServer
{
    public class RoomServerElement
    {
        private ProcessHandeler process;
        private Thread thread;
        private Socket host;
        private Socket tablet;

        private bool isReady;
        private bool isRunnig;
        private string name;
        private string password;
        private string port;


        public RoomServerElement(ProcessHandeler process, Thread thread)
        {
            this.Process = process;
            this.Thread = thread;
            Port = Process.RoomServerPort;
            OpenForGuest();
        }
        public RoomServerElement(string name, string password)
        {
            Name = name;
            Password = password;
            OpenForTablet();
        }

        public RoomServerElement(string name, string password, bool run)
        {
            Name = name;
            Password = password;
            if (run) OpenForGuest();
            else OpenForTablet();
        }

        public void SetRoomServerElements(ProcessHandeler process, Thread thread)
        {
            this.Process = process;
            this.Thread = thread;
            Port = Process.RoomServerPort;
            OpenForGuest();
        }

        public ProcessHandeler Process { get => process; set => process = value; }
        public Thread Thread { get => thread; set => thread = value; }
        public Socket Host { get => host; set => host = value; }
        public Socket Tablet { get => tablet; set => tablet = value; }
        public string Name { get => name; set => name = value; }
        public string Password { get => password; set => password = value; }
        public string Port { get => port; set => port = value; }
        public bool IsReady { get => isReady; set => isReady = value; }
        public bool IsRunnig { get => isRunnig; set => isRunnig = value; }


        public void OpenForTablet()
        {
            IsReady = true;
            IsRunnig = false;
        }
        public void OpenForGuest()
        {
            IsReady = false;
            IsRunnig = true;
        }
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
                for (int i = 0; i < Rooms.Count; i++)
                {
                    if (Rooms[i].IsRunnig)
                    {
                        if (Rooms[i].Thread.Join(timeSlice))
                        {
                            Rooms.RemoveAt(i);
                            if (Rooms.Count == 0) break;
                        }
                    }
                    if (!Rooms[i].Host.Connected)
                    {
                        Rooms.RemoveAt(i);
                        if (Rooms.Count == 0) break;
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
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (Clients[i].Thread.Join(timeSlice))
                    {
                        Clients.RemoveAt(i);
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
}

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
