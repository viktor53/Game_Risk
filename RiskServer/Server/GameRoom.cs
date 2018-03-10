using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Server
{
  internal class GameRoom : IGameRoom
  {
    private Game _game;

    private GameBoardInfo _gameInfo;

    private RiskServer _server;

    private object _playersLock;

    private Dictionary<string, IClientManager> _players;

    private int _capacity;

    public string RoomName { get; private set; }

    public int Capacity
    {
      get
      {
        return _capacity;
      }
    }

    public int Connected
    {
      get
      {
        return _players.Count;
      }
    }

    public GameRoom(string roomName, int capacity, bool isClassic, RiskServer server)
    {
      RoomName = roomName;
      _capacity = capacity < 3 ? 3 : capacity;
      _game = new Game(isClassic, _capacity);
      _playersLock = new object();
      _players = new Dictionary<string, IClientManager>();
      _server = server;
    }

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
          _players.Add(player.PlayerName, player);
          player.PlayerColor = Model.Enums.ArmyColor.Green + _players.Count;

          return true;
        }
        return false;
      }
    }

    public IList<string> GetPlayers()
    {
      return _players.Keys.ToList();
    }

    public GameBoardInfo GetBoardInfo(GamePlanInfo gamePlan)
    {
      if (_gameInfo != null)
      {
        return _gameInfo;
      }
      else
      {
        return CreateBoardInfo(gamePlan);
      }
    }

    private GameBoardInfo CreateBoardInfo(GamePlanInfo gamePlan)
    {
      const int width = 1920;
      const int height = 1080;

      int X = (width - 100) / 3;
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
            mark = xa >= 2 * X;
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
        areasInfo.Add(new AreaInfo(c, gamePlan.Areas[i]));
      }

      _gameInfo = new GameBoardInfo(gamePlan.Connections, areasInfo);
      return _gameInfo;
    }

    private int GetDistance(int x1, int y1, int x2, int y2)
    {
      return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    private bool IsCorrect(int x, int y, List<Coordinates> coords)
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

    public bool IsFull()
    {
      return _players.Count == _capacity;
    }

    public void LeaveGame(string playerName)
    {
      lock (_playersLock)
      {
        _players.Remove(playerName);
        foreach (var player in _players)
        {
          player.Value.SendPlayerLeave(playerName);
        }
      }
    }

    public Task StartGame()
    {
      return Task.Run(async () =>
      {
        bool isAllReady = true;
        foreach (var player in _players)
        {
          isAllReady = isAllReady && await player.Value.WaitUntilPlayerIsReady();
        }

        if (isAllReady)
        {
          foreach (var player in _players)
          {
            _game.AddPlayer(player.Value);
          }

          _game.StartGame();
        }
      });
    }
  }
}