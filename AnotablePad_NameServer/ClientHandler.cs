using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AnotablePad_NameServer
{
    public class ClientHandler
    {
        Socket sock;
        bool isTablet;
        byte[] recvBuffer;
        byte[] sendBuffer;
        public ClientHandler(Socket sock)
        {
            this.Sock = sock;
            this.recvBuffer = new byte[1024];
        }
        public Socket Sock { get => sock; set => sock = value; }
        public bool IsTablet { get => isTablet; set => isTablet = value; }

        public void RunClientHandler()
        {
            try
            {
                int recvSize = Sock.Receive(recvBuffer, recvBuffer.Length, SocketFlags.None);
                if (recvSize > 0)
                {
                    string message = Encoding.UTF8.GetString(recvBuffer, 0, recvSize);
                    if (message == (AppData.ServerCommand + "Tablet" + AppData.Delimiter))
                    {
                        Console.WriteLine("Tablet is Connected");
                        IsTablet = true;
                    }
                    else
                    {
                        Console.WriteLine("PC is Connected");
                        IsTablet = false;
                    }
                }

                sendBuffer = Encoding.UTF8.GetBytes(AppData.ServerCommand + "CONNECTION");
                Sock.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                while (true)
                {
                    recvSize = Sock.Receive(recvBuffer, recvBuffer.Length, SocketFlags.None);
                    if (recvSize > 0)
                    {
                        string message = Encoding.UTF8.GetString(recvBuffer, 0, recvSize);
                        string returnMessage = ReturnRequest(message);
                        if (returnMessage == "") continue;
                        sendBuffer = Encoding.UTF8.GetBytes(returnMessage);
                        Sock.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                    }
                    else
                    {
                        sendBuffer = Encoding.UTF8.GetBytes(AppData.ServerCommand + "ERROR");
                        Sock.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                        break;
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket Disconnect");
            }
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
            }
        }

        private string ReturnRequest(string request)
        {
            string msg = "";

            if (request == CommendBook.FIND_ROOM)
            {
                msg = MakeRoomList(IsTablet);
            }
            else if (request.Contains(CommendBook.CREATE_ROOM))
            {
                msg = CreateRoomElement(request);
            }
            else if (request.Contains(CommendBook.ENTER_ROOM))
            {
                msg = EnterRoom(request);
            }
            else
            {
                msg = CommendBook.ERROR_MESSAGE + "COMMAND";
            }
            return msg;
        }

        private string MakeRoomList(bool tablet)
        {
            string roomlist = CommendBook.HEADER_ROOMLIST;
            if (tablet)
            {
                foreach (var room in AppData.Rooms)
                {
                    if (!room.IsReady) continue;
                    roomlist += AppData.DelimiterUI + room.Name;
                }
            }
            else
            {
                foreach (var room in AppData.Rooms)
                {
                    if (!room.IsRunnig) continue;
                    roomlist += AppData.DelimiterUI + room.Name;
                }
            }
            return roomlist;
        }

        private string CreateRoomElement(string request)
        {
            if (IsTablet) return CommendBook.ERROR_MESSAGE + "INVALID";

            var data = request.Split(AppData.DelimiterUI);

            foreach (var room in AppData.Rooms)
                if (room.Name == data[1]) return CommendBook.ERROR_MESSAGE + "NAME";

            var newRoom = new RoomServerElement(data[1], data[2]);
            newRoom.Host = this.Sock;
            AppData.Rooms.Add(newRoom);
            return CommendBook.CREATE_ROOM + AppData.DelimiterUI + newRoom.Name;
        }

        private string EnterRoom(string request)
        {
            if (IsTablet)
            {
                var data = request.Split(AppData.DelimiterUI);
                foreach (var room in AppData.Rooms)
                {
                    if (!room.IsReady) continue;
                    if (room.Name != data[1]) continue;
                    if (room.Password != data[2]) return CommendBook.ERROR_MESSAGE + "WRONGPW";

                    sendBuffer = Encoding.UTF8.GetBytes(CommendBook.START_DRAWING);
                    Sock.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                    room.Host.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                    ProcessHandeler roomProcess = new ProcessHandeler(room.Host, this.Sock, room.Name);
                    Thread roomHandler = new Thread(new ThreadStart(roomProcess.RunProcess));
                    room.SetRoomServerElements(roomProcess, roomHandler);
                    roomHandler.Start();

                    return "";
                }
                return CommendBook.ERROR_MESSAGE + "NOROOM";
            }
            else
            {
                var data = request.Split(AppData.DelimiterUI);
                foreach (var room in AppData.Rooms)
                {
                    if (!room.IsRunnig) continue;
                    if (room.Name != data[1]) continue;
                    if (room.Password != data[2]) return CommendBook.ERROR_MESSAGE + "WRONGPW";

                    sendBuffer = Encoding.UTF8.GetBytes(CommendBook.GUEST_DRAWING);
                    Sock.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);

                    Thread.Sleep(100);

                    var port = Encoding.UTF8.GetBytes(room.Port);
                    Sock.Send(port, port.Length, SocketFlags.None);
                    return "";
                }
                return CommendBook.ERROR_MESSAGE + "NOROOM";
            }
        }
    }
}
