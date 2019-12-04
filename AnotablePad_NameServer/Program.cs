
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AnotablePad_NameServer
{
    public class LobbyServer
    {
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

                Console.WriteLine("MuliThread Starting : Waiting for connections...");

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
                observerThread.Join();
            }

            Console.WriteLine("AnotablePad NameServer is Closed...");
        }
    }
}