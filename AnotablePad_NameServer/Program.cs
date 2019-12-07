
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AnotablePad_NameServer
{
    public class NameServer
    {
        /// <summary>
        /// NameServer는 계속 Accept를 유지하면서 새로운 접속이 발생하면 바로 Thread를 만들어서 처리한다.
        /// </summary>
        public static void Main()
        {
            List<RoomServerElement> rooms = AppData.Rooms;
            List<ClientElement> clients = AppData.Clients;

            TcpListener tcpListener = null;
            ThreadObserver observer = new ThreadObserver(rooms, clients);
            Thread observerThread = new Thread(new ThreadStart(observer.runObserving));
            observerThread.Start();

            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 4444);

                tcpListener.Start();

                Console.WriteLine("AnotablePad Server is Ready : Waiting for connections...");

                while (true)
                {
                    Socket temp = tcpListener.AcceptSocket();
                    ClientHandler clientHandler = new ClientHandler(temp);
                    Thread clientThread = new Thread(new ThreadStart(clientHandler.RunClientHandler));
                    clients.Add(new ClientElement(clientHandler, clientThread));
                    clientThread.Start();
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception :" + exp);
            }
            finally
            {
                tcpListener.Stop();
                observer.Loop = false;
                observerThread.Join();
            }

            Console.WriteLine("AnotablePad Server is Closed...");
        }
    }
}