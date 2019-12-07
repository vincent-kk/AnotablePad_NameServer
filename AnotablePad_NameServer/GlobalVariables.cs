using System.Collections.Generic;
/// <summary>
/// 자주 사용하거나 공통적으로 사용되는 부분을 정리해둔 부분
/// </summary>
namespace AnotablePad_NameServer
{
    public class AppData
    {
        private static List<RoomServerElement> rooms = new List<RoomServerElement>();
        private static List<ClientElement> clients = new List<ClientElement>();
        private static readonly char delimiter = '|';
        private static readonly char delimiterUI = '%';
        private static readonly char clientCommand = '#';
        private static readonly char serverCommand = '@';
        public static List<RoomServerElement> Rooms { get => rooms; set => rooms = value; }
        public static List<ClientElement> Clients { get => clients; set => clients = value; }
        public static char Delimiter => delimiter;
        public static char DelimiterUI => delimiterUI;
        public static char ClientCommand => clientCommand;
        public static char ServerCommand => serverCommand;
    }

    public static class CommendBook
    {
        private static readonly string findRoom = AppData.ServerCommand + "FIND-ROOM" + AppData.Delimiter;
        private static readonly string createRoom = AppData.ServerCommand + "CREATE-ROOM";
        private static readonly string enterRoom = AppData.ServerCommand + "ENTER-ROOM";
        private static readonly string startDrawing = AppData.ServerCommand + "START-DRAWING";
        private static readonly string guestDrawing = AppData.ServerCommand + "GUEST-DRAWING";
        private static readonly string errorMessage = AppData.ServerCommand + "ERROR" + AppData.DelimiterUI;
        private static readonly string roomListHeader = AppData.ServerCommand + "ROOM-LIST";
        private static readonly string connection = AppData.ServerCommand + "CONNECTION";
        public static string FIND_ROOM => findRoom;
        public static string CREATE_ROOM => createRoom;
        public static string ENTER_ROOM => enterRoom;
        public static string HEADER_ROOMLIST => roomListHeader;
        public static string ERROR_MESSAGE => errorMessage;
        public static string START_DRAWING => startDrawing;
        public static string GUEST_DRAWING => guestDrawing;
        public static string Connection => connection;
    }
}
