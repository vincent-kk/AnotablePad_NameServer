using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace AnotablePad_NameServer
{
    /// <summary>
    /// Room Server를 구성하는 다양한 정보를 모아둔 클래스
    /// 기본적으로 구조체와 유사하게 사용된다.
    /// </summary>
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


        public RoomServerElement(string name, string password)
        {
            Name = name;
            Password = password;
            OpenForTablet();
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
    /// <summary>
    /// Room Server와 동일. 접속한 사용자에 대한 정보를 모아두는 클래스
    /// </summary>
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
    /// <summary>
    /// 일종의 GC역할을 하는 Thread이다.
    /// List를 순회하며 연결이 끊긴 Client Thread나 Room Server Thread를 정리한다.
    /// </summary>
    class ThreadObserver
    {
        private bool loop;
        private Thread thread;
        private List<RoomServerElement> rooms;
        private List<ClientElement> clients = new List<ClientElement>();
        private readonly int timeScale = 1000;
        public Thread Thread { get => thread; set => thread = value; }
        public List<RoomServerElement> Rooms { get => rooms; set => rooms = value; }
        public List<ClientElement> Clients { get => clients; set => clients = value; }
        public bool Loop { get => loop; set => loop = value; }

        public ThreadObserver(List<RoomServerElement> rooms, List<ClientElement> clients)
        {
            Rooms = rooms;
            Clients = clients;
            Loop = true;
        }
        public void runObserving()
        {
            Console.WriteLine("Garbage Collector Strating...");
            while (Loop)
            {
                CleanRoom();
                CleanClient();
            }
        }

        /// <summary>
        /// Room Thread 정리.
        /// 일정한 Time Scale을 List의 크기로 나눠서 사용한다.
        /// 동일한 시간마다 동작하게 하기 위한 구성
        /// </summary>
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
                            Console.WriteLine("Room Name : {0} Clean Up", Rooms[i].Name);
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
        /// <summary>
        /// Client Thread 정리.
        /// 일정한 Time Scale을 List의 크기로 나눠서 사용한다.
        /// 동일한 시간마다 동작하게 하기 위한 구성
        /// </summary>
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