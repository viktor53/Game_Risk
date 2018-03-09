using Risk.Networking.Messages;
using Risk.Networking.Messages.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Risk.Networking.Server
{
  public class RiskServer
  {
    private ManualResetEvent _allDone = new ManualResetEvent(false);

    private string _hostNameOrAddress;

    private int _port;

    private int _maxLengthConQueue;

    private object _playersLock;

    private IDictionary<string, IClientManager> _players;

    private HashSet<IClientManager> _playersInMenu;

    private object _gameRoomsLock;

    private IDictionary<string, IGameRoom> _gameRooms;

    public RiskServer() : this("localhost", 1100, 100)
    {
    }

    public RiskServer(int port) : this("localhost", port, 100)
    {
    }

    public RiskServer(string hostNameOrAddress, int port) : this(hostNameOrAddress, port, 100)
    {
    }

    public RiskServer(string hostNameOrAddress, int port, int maxLengthConQueue)
    {
      _hostNameOrAddress = hostNameOrAddress;
      _port = port;
      _maxLengthConQueue = maxLengthConQueue;
      _playersLock = new object();
      _players = new Dictionary<string, IClientManager>();
      _gameRoomsLock = new object();
      _gameRooms = new Dictionary<string, IGameRoom>();
      _playersInMenu = new HashSet<IClientManager>();

      Debug.WriteLine("**Server inicialization: host={0} port={1} maxLengthConQueue={2} OK", _hostNameOrAddress, _port, _maxLengthConQueue);
    }

    public void Start()
    {
      Debug.WriteLine("**Server: Starting listenig");

      IPAddress ipHost = Dns.GetHostEntry(_hostNameOrAddress).AddressList[0];
      IPEndPoint localEnd = new IPEndPoint(ipHost, _port);

      Socket listener = new Socket(ipHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

      try
      {
        listener.Bind(localEnd);
        listener.Listen(_maxLengthConQueue);

        while (true)
        {
          _allDone.Reset();

          listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

          _allDone.WaitOne();
        }
      }
      catch (Exception e)
      {
        Debug.WriteLine("**Server ERROR: " + e.StackTrace);
      }
    }

    public void AcceptCallback(IAsyncResult result)
    {
      Debug.WriteLine("**Server: NEW CLIENT");

      _allDone.Set();

      Socket listener = (Socket)result.AsyncState;
      Socket handler = listener.EndAccept(result);

      IClientManager client = new Player(handler, this);
      client.SartListening();
    }

    public bool AddPlayer(string name, IClientManager player)
    {
      lock (_playersLock)
      {
        if (!_players.ContainsKey(name))
        {
          Debug.WriteLine($"** Add new Player: {name}", "Server");
          _players.Add(name, player);
          _playersInMenu.Add(player);
          return true;
        }
        return false;
      }
    }

    public void LogOutPlayer(string name)
    {
      lock (_playersLock)
      {
        Debug.WriteLine($"** LogOut player: {name}", "Server");
        _playersInMenu.Remove(_players[name]);
        _players.Remove(name);
      }
    }

    public bool CreateGame(CreateGameRoomInfo gameRoom, string playerName)
    {
      bool result = false;
      lock (_gameRoomsLock)
      {
        if (!_gameRooms.ContainsKey(gameRoom.RoomName))
        {
          Debug.WriteLine($"** Create new Room: {gameRoom.RoomName}", "Server");
          _gameRooms.Add(gameRoom.RoomName, new GameRoom(gameRoom.RoomName, gameRoom.Capacity, gameRoom.IsClassic));
          _gameRooms[gameRoom.RoomName].AddPlayer(_players[playerName]);
          _playersInMenu.Remove(_players[playerName]);

          result = true;
        }
      }
      SendUpdateToAll();
      return result;
    }

    public bool ConnectToGame(string playerName, string gameName)
    {
      if (_gameRooms.ContainsKey(gameName) && _gameRooms[gameName].AddPlayer(_players[playerName]))
      {
        _playersInMenu.Remove(_players[playerName]);
        if (_gameRooms[gameName].IsFull())
        {
          _gameRooms[gameName].StartGame();
          lock (_gameRoomsLock)
          {
            _gameRooms.Remove(gameName);
          }
        }
        SendUpdateToAll();
        return true;
      }
      return false;
    }

    private async void SendUpdateToAll()
    {
      await Task.Run(() =>
      {
        List<GameRoomInfo> roomsInfo = GetUpdateInfo();

        foreach (var player in _playersInMenu)
        {
          player.SendUpdateGameList(roomsInfo);
        }
      });
    }

    public List<GameRoomInfo> GetUpdateInfo()
    {
      List<GameRoomInfo> roomsInfo = new List<GameRoomInfo>();

      lock (_gameRoomsLock)
      {
        foreach (var room in _gameRooms)
        {
          roomsInfo.Add(new GameRoomInfo(room.Value.RoomName, room.Value.Capacity, room.Value.Connected));
        }
        roomsInfo.Add(new GameRoomInfo("test1", 6, 3));
        roomsInfo.Add(new GameRoomInfo("test2", 5, 2));
      }

      return roomsInfo;
    }
  }
}