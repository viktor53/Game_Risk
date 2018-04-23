using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using Risk.Networking.Messages.Data;
using Risk.Model.GameCore.Loggers;

namespace Risk.Networking.Server
{
  /// <summary>
  /// Represents game room, where players can connect and play the game.
  /// </summary>
  internal class GameRoom : IGameRoom
  {
    private Game _game;

    private GameBoardInfo _gameInfo;

    private RiskServer _server;

    private object _playersLock;

    private Dictionary<string, IClientManager> _players;

    private int _capacity;

    private int _ready;

    public string RoomName { get; private set; }

    public event EventHandler OnStart;

    public int Capacity => _capacity;

    public int Connected => _players.Count;

    /// <summary>
    /// Creates game room with name, maximum capacity and specification if it is classic game board.
    /// </summary>
    /// <param name="roomName">name of game room</param>
    /// <param name="capacity">maximum capacity</param>
    /// <param name="isClassic">if it is classic game room or random generated</param>
    /// <param name="server">risk server</param>
    public GameRoom(string roomName, int capacity, bool isClassic, RiskServer server)
    {
      RoomName = roomName;
      _capacity = capacity < 3 ? 3 : capacity;
      var date = DateTime.Now;

      ILog gameLogger = Loggers.GetDateFileLogger(AppDomain.CurrentDomain.BaseDirectory + "Logs\\Games");

      _game = new Game(isClassic, _capacity, gameLogger);
      _playersLock = new object();
      _players = new Dictionary<string, IClientManager>();
      _server = server;
      _ready = 0;
    }

    /// <summary>
    /// Adds new player into game room.
    /// </summary>
    /// <param name="player">new player</param>
    /// <returns>true if player is added otherwise false</returns>
    public bool AddPlayer(IClientManager player)
    {
      lock (_playersLock)
      {
        if (!IsFull())
        {
          foreach (var pl in _players)
          {
            pl.Value.SendNewPlayerConnected(player.PlayerName);
          }
          player.PlayerColor = Model.Enums.ArmyColor.Green + _players.Count;
          _players.Add(player.PlayerName, player);
          player.OnReady += OnReady;
          player.ListenToReadyTag();
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Removes the player from the game room.
    /// </summary>
    /// <param name="playerName">name of player that will be removed</param>
    public void RemovePlayer(string playerName)
    {
      lock (_playersLock)
      {
        _ready = 0;
        _players[playerName].OnReady -= OnReady;
        _players.Remove(playerName);
        foreach (var player in _players)
        {
          player.Value.SendPlayerLeave(playerName);
          player.Value.ListenToReadyTag();
        }
      }
    }

    /// <summary>
    /// Gets all connected players.
    /// </summary>
    /// <returns>connected players</returns>
    public IList<string> GetPlayers()
    {
      return _players.Keys.ToList();
    }

    /// <summary>
    /// If game room is full or not.
    /// </summary>
    /// <returns>if game room is full or not</returns>
    public bool IsFull()
    {
      return _players.Count == _capacity;
    }

    /// <summary>
    /// Gets game board information. If it does not exist it is created.
    /// </summary>
    /// <param name="gamePlan">game plan information</param>
    /// <returns>game board information</returns>
    public GameBoardInfo GetBoardInfo(GamePlanInfo gamePlan)
    {
      if (_gameInfo == null)
      {
        _gameInfo = CreateBoardInfo(gamePlan);
      }
      return _gameInfo;
    }

    /// <summary>
    /// Creates game board information for client from game plan inforamtion.
    /// </summary>
    /// <param name="gamePlan">game plan information</param>
    /// <returns>game board information from game plan information</returns>
    public static GameBoardInfo CreateBoardInfo(GamePlanInfo gamePlan)
    {
      // size of area where points will be placed
      const int width = 1920;
      const int height = 1080;

      int numberOfregions = GetNumberOfRegions(gamePlan);
      int areasInRow = numberOfregions / 2 + numberOfregions % 2;

      // dividing area into smaller areas that represent region
      int X = (width - 100) / areasInRow;
      int Y = (height - 200) / 2;
      int g = 3000;

      List<AreaInfo> areasInfo = new List<AreaInfo>();

      int Ym = g;
      int Xm = (int)Math.Round((double)(g * X / Y));

      Random ran = new Random();
      bool mark = false;

      int xa = 0;
      int ya = 0;

      List<Coordinates> coords = new List<Coordinates>();

      // creating random points with the specific distance from other points
      int prevReg = gamePlan.Areas[0].RegionID;
      for (int i = 0; i < gamePlan.Areas.Length; ++i)
      {
        if (prevReg != gamePlan.Areas[i].RegionID)
        {
          prevReg = gamePlan.Areas[i].RegionID;
          if (mark)
          {
            xa = 0;
            ya += Y;
            mark = false;
          }
          else
          {
            xa += X;
            mark = xa >= (areasInRow - 1) * X;
          }
        }

        bool correct = false;
        int a = 0;
        int b = 0;

        while (!correct)
        {
          a = X * ran.Next(Xm) / Xm + xa;
          b = Y * ran.Next(Ym) / Ym + ya;
          correct = IsCorrect(a, b, coords);
        }
        var c = new Coordinates(a, b);
        coords.Add(c);
        areasInfo.Add(new AreaInfo(c, gamePlan.Areas[i], ran.Next(1, 9)));
      }

      return new GameBoardInfo(gamePlan.Connections, areasInfo);
    }

    /// <summary>
    /// Counts number of regions.
    /// </summary>
    /// <param name="gamePlan">game plan inforamtion</param>
    /// <returns>number of regions</returns>
    private static int GetNumberOfRegions(GamePlanInfo gamePlan)
    {
      int count = 0;
      int prev = -1;
      for (int i = 0; i < gamePlan.Areas.Length; ++i)
      {
        if (prev != gamePlan.Areas[i].RegionID)
        {
          count++;
          prev = gamePlan.Areas[i].RegionID;
        }
      }
      return count;
    }

    /// <summary>
    /// Counts distance between two points in 2D.
    /// </summary>
    /// <param name="x1">X coordinate of point A</param>
    /// <param name="y1">Y coordinate of point A</param>
    /// <param name="x2">X coordinate of point B</param>
    /// <param name="y2">Y coordinate of point B</param>
    /// <returns>distance between two points</returns>
    private static int GetDistance(int x1, int y1, int x2, int y2)
    {
      return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    /// <summary>
    /// Checks if point has correct distance from other already placed points.
    /// </summary>
    /// <param name="x">X coordinate of point</param>
    /// <param name="y">Y coordinate of point</param>
    /// <param name="coords">coordinates of already placed points</param>
    /// <returns>if point is correct</returns>
    private static bool IsCorrect(int x, int y, List<Coordinates> coords)
    {
      foreach (var co in coords)
      {
        if (GetDistance(x, y, co.X, co.Y) < 110)
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Method that is called when OnReady event is raised. If all players are ready, it will start game.
    /// </summary>
    /// <param name="sender">the player, who raised the event</param>
    /// <param name="ev">EventArgs is not used</param>
    private void OnReady(object sender, EventArgs ev)
    {
      if (++_ready == _capacity)
      {
        OnStart?.Invoke(this, new EventArgs());
        StartAsync();
      }
    }

    /// <summary>
    /// Asynchronously starts game. Firstable adds ale players into game and in the end remove all players.
    /// </summary>
    private async void StartAsync()
    {
      await Task.Run(() =>
      {
        foreach (var player in _players)
        {
          _game.AddPlayer(player.Value);
        }

        _game.StartGame();

        foreach (var player in _players)
        {
          _server.LeaveGame(player.Value.PlayerName, RoomName);
        }
      });
    }
  }
}