using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AnotablePad_NameServer
{
    /// <summary>
    /// Client가 실제로 Server와 통신하는 부분.
    /// buffer를 사용한 방식이 아닌 Blocking 방식으로 구현되어있다.
    /// Client의 입력에 대한 Server의 답변으로 구성되기 때문.
    /// </summary>
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
                    IsTablet = (message == (AppData.ServerCommand + "Tablet" + AppData.Delimiter));
                }

                sendBuffer = Encoding.UTF8.GetBytes(CommendBook.Connection);
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
                        sendBuffer = Encoding.UTF8.GetBytes(CommendBook.ERROR_MESSAGE);
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
        /// <summary>
        /// Client의 요청에 대한 결과를 정리해둔 부분
        /// 지정된 요청 이외의 요청은 모두 에러로 처리한다.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 현재 접속 가능한 방을 찾아서 결과를 만드는 부분.
        /// Tablet의 요청인지, PC의 요청인지에 따라 다른 동작을 한다.
        /// </summary>
        /// <param name="tablet"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 새 방을 만든다.
        /// Tablet은 방을 생성할 수 없다.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string CreateRoomElement(string request)
        {
            if (IsTablet) return CommendBook.ERROR_MESSAGE;

            var data = request.Split(AppData.DelimiterUI);

            foreach (var room in AppData.Rooms)
                if (room.Name == data[1]) return CommendBook.ERROR_MESSAGE + "NAME";

            var newRoom = new RoomServerElement(data[1], data[2]);
            newRoom.Host = this.Sock;
            AppData.Rooms.Add(newRoom);
            return CommendBook.CREATE_ROOM + AppData.DelimiterUI + newRoom.Name;
        }
        /// <summary>
        /// 방에 접속한다. Tablet은 방마다 단 하나 존재하므로 Tablet이 접속해야 방이 동작한다.
        /// PC의 경우 방에 입장하는 경우가 Guest로 입장하는 경우 밖에 없다.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
