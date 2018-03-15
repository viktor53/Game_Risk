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
using log4net;
using log4net.Config;
using System.IO;

namespace Risk.Networking.Server
{
  public class RiskServer
  {
    private readonly ILog _logger;

    private ManualResetEvent _allDone = new ManualResetEvent(false);

    private string _hostNameOrAddress;

    private int _port;

    private int _maxLengthConQueue;

    private object _playersLock;

    private IDictionary<string, IClientManager> _players;

    private HashSet<IClientManager> _playersInMenu;

    private object _gameRoomsLock;

    private IDictionary<string, IGameRoom> _gameRooms;

    public RiskServer() : this("localhost", 1100, 100, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"))
    {
    }

    public RiskServer(int port) : this("localhost", port, 100, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"))
    {
    }

    public RiskServer(string hostNameOrAddress, int port) : this(hostNameOrAddress, port, 100, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"))
    {
    }

    public RiskServer(string hostNameOrAddress, int port, int maxLengthConQueue, FileInfo pathToConfig)
    {
      _hostNameOrAddress = hostNameOrAddress;
      _port = port;
      _maxLengthConQueue = maxLengthConQueue;

      XmlConfigurator.Configure(pathToConfig);

      _logger = LogManager.GetLogger("ServerLogger");

      _playersLock = new object();
      _players = new Dictionary<string, IClientManager>();
      _gameRoomsLock = new object();
      _gameRooms = new Dictionary<string, IGameRoom>();
      _playersInMenu = new HashSet<IClientManager>();

      _logger.Info($"New instance of server: {DateTime.Now.ToLocalTime()}");
      _logger.Info($"Server inicialization: host={_hostNameOrAddress} port={_port} maxLengthConQueue={_maxLengthConQueue}");
    }

    public void Start()
    {
      _logger.Info("Server is starting to listen");

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
        _logger.Error($"Message: {e.Message} \n StackTrace: {e.StackTrace}");
      }
    }

    public void AcceptCallback(IAsyncResult result)
    {
      _logger.Info("Accepting new client");

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
          _logger.Info($"Registrating new player: {name}");

          _players.Add(name, player);
          _playersInMenu.Add(player);
          player.OnLeave += OnLeave;
          return true;
        }
        return false;
      }
    }

    public void LogOutPlayer(string name)
    {
      lock (_playersLock)
      {
        if (_players.ContainsKey(name))
        {
          _logger.Info($"Logout player: {name}");

          _playersInMenu.Remove(_players[name]);
          _players.Remove(name);
        }
      }
    }

    public void LeaveGame(string name, string gameName)
    {
      if (_gameRooms.ContainsKey(gameName))
      {
        _logger.Info($"Player {name} is leaving game room {gameName}");
        lock (_gameRoomsLock)
        {
          _gameRooms[gameName].LeaveGame(name);
          _playersInMenu.Add(_players[name]);
          if (_gameRooms[gameName].Connected == 0)
          {
            _logger.Info($"Removing empty game room {gameName}");
            _gameRooms.Remove(gameName);
          }
        }
      }
      else
      {
        if (!_playersInMenu.Contains(_players[name])) _playersInMenu.Add(_players[name]);
      }
    }

    private void OnLeave(object sender, EventArgs ev)
    {
      IClientManager client = (IClientManager)sender;
      LeaveGame(client.PlayerName, client.GameRoom.RoomName);
      client.GameRoom = null;
    }

    private void OnStart(object sender, EventArgs ev)
    {
      IGameRoom room = (IGameRoom)sender;
      lock (_gameRoomsLock)
      {
        _logger.Info($"Game room {room.RoomName} is starting play");

        _gameRooms.Remove(room.RoomName);
      }
    }

    public bool CreateGame(CreateGameRoomInfo gameRoom, string playerName)
    {
      bool result = false;
      lock (_gameRoomsLock)
      {
        if (!_gameRooms.ContainsKey(gameRoom.RoomName))
        {
          _logger.Info($"Creating new game room {gameRoom.RoomName} for {gameRoom.Capacity} players, classic: {gameRoom.IsClassic}");

          _gameRooms.Add(gameRoom.RoomName, new GameRoom(gameRoom.RoomName, gameRoom.Capacity, gameRoom.IsClassic, this));
          _gameRooms[gameRoom.RoomName].OnStart += OnStart;
          _gameRooms[gameRoom.RoomName].AddPlayer(_players[playerName]);
          _playersInMenu.Remove(_players[playerName]);
          _players[playerName].GameRoom = _gameRooms[gameRoom.RoomName];

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
        _logger.Info($"Player {playerName} is connecting to game room {gameName}");

        _players[playerName].GameRoom = _gameRooms[gameName];
        _playersInMenu.Remove(_players[playerName]);
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
      }

      return roomsInfo;
    }
  }
}