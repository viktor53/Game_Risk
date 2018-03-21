using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using log4net;
using log4net.Config;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Server
{
  /// <summary>
  /// Represents risk server. Accepts connection a creates server side players.
  /// </summary>
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

    /// <summary>
    /// Creates default risk server listening on localhost with port 11000 and with default configuration of logger.
    /// </summary>
    public RiskServer() : this("localhost", 11000, 100, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"))
    {
    }

    /// <summary>
    /// Creates risk server listening on localhost with the specific port and with default configuration of logger.
    /// </summary>
    /// <param name="port">port where to listen</param>
    public RiskServer(int port) : this("localhost", port, 100, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"))
    {
    }

    /// <summary>
    /// Creates risk server listening on specific address and port and with default configuration of logger.
    /// </summary>
    /// <param name="hostNameOrAddress">host name or address where to listen</param>
    /// <param name="port">port where to listen</param>
    public RiskServer(string hostNameOrAddress, int port) : this(hostNameOrAddress, port, 100, new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DefaultLogger.xml"))
    {
    }

    /// <summary>
    /// Constructor with all parameters of server.
    /// </summary>
    /// <param name="hostNameOrAddress">host name or address where to listen</param>
    /// <param name="port">port where to listen</param>
    /// <param name="maxLengthConQueue">maximum length of connection queue</param>
    /// <param name="pathToConfig">path to configuration of logger containing logger with name ServerLogger</param>
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

    /// <summary>
    /// Starts accepting connections.
    /// </summary>
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

    /// <summary>
    /// Accepts connections and creates server side client and starts listening.
    /// </summary>
    /// <param name="result">async result listener</param>
    public void AcceptCallback(IAsyncResult result)
    {
      _logger.Info("Accepting new client");

      _allDone.Set();

      Socket listener = (Socket)result.AsyncState;
      Socket handler = listener.EndAccept(result);

      IClientManager client = new Player(handler, this);
      client.SartListening();
    }

    /// <summary>
    /// Adds player into connected players and players in menu.
    /// </summary>
    /// <param name="name">name of player</param>
    /// <param name="player">client manager</param>
    /// <returns>true if player is added, false if player with the name already exists</returns>
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

    /// <summary>
    /// Logs out player.
    /// </summary>
    /// <param name="name">name of player</param>
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

    /// <summary>
    /// Removes the player from the game room.
    /// </summary>
    /// <param name="name">name of player</param>
    /// <param name="gameName">name of game room</param>
    public void LeaveGame(string name, string gameName)
    {
      if (_gameRooms.ContainsKey(gameName))
      {
        _logger.Info($"Player {name} is leaving game room {gameName}");
        lock (_gameRoomsLock)
        {
          _gameRooms[gameName].RemovePlayer(name);
          _playersInMenu.Add(_players[name]);
          if (_gameRooms[gameName].Connected == 0)
          {
            _logger.Info($"Removing empty game room {gameName}");
            _gameRooms.Remove(gameName);
          }
        }
        SendUpdateToAll();
      }
      else
      {
        if (!_playersInMenu.Contains(_players[name])) _playersInMenu.Add(_players[name]);
      }
    }

    /// <summary>
    /// Method that is called when OnLeave event is raised. Removes the sender from the game room.
    /// </summary>
    /// <param name="sender">the player, who raised the event</param>
    /// <param name="ev">EventArgs is not used</param>
    private void OnLeave(object sender, EventArgs ev)
    {
      IClientManager client = (IClientManager)sender;
      LeaveGame(client.PlayerName, client.GameRoom.RoomName);
      client.GameRoom = null;
    }

    /// <summary>
    /// Method that is called when OnStart event is raised. Removes started game room from game room list.
    /// </summary>
    /// <param name="sender">the game room, who raised the event</param>
    /// <param name="ev">EventArgs is not used</param>
    private void OnStart(object sender, EventArgs ev)
    {
      IGameRoom room = (IGameRoom)sender;
      lock (_gameRoomsLock)
      {
        _logger.Info($"Game room {room.RoomName} is starting play");

        _gameRooms.Remove(room.RoomName);
      }
      SendUpdateToAll();
    }

    /// <summary>
    /// Creates game room if it does not exist yet.
    /// </summary>
    /// <param name="gameRoom">information about new game room</param>
    /// <param name="playerName">name of player that want to create the game room</param>
    /// <returns>true if the game room is created, false if the game room already exists</returns>
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

    /// <summary>
    /// Connects the player to the game room.
    /// </summary>
    /// <param name="playerName">name of player</param>
    /// <param name="gameName">name of game room</param>
    /// <returns>true if player is connected, false if game room is full</returns>
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

    /// <summary>
    /// Asynchronously sends update of game room list to all players in menu.
    /// </summary>
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

    /// <summary>
    /// Gets information about game room list.
    /// </summary>
    /// <returns>game room information list</returns>
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