using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal abstract class State
  {
    protected GameBoard _gameBoard;

    protected IList<IPlayer> _players;

    protected IDictionary<ArmyColor, Game.PlayerInfo> _playersInfo;

    protected int _currentPlayer;

    protected Phase _currentPhase;

    protected StatusOfGame _status;

    public Moves Moves { get; set; }

    public int CurrentPlayer
    {
      get
      {
        return _currentPlayer;
      }
      set
      {
        _currentPlayer = value;
      }
    }

    public StatusOfGame Status
    {
      get
      {
        return _status;
      }
      set
      {
        _status = value;
      }
    }

    public State(GameBoard gameBoard, IList<IPlayer> players, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, int currentPlayer, Phase currentPhase)
    {
      _gameBoard = gameBoard;
      _players = players;
      _playersInfo = playersInfo;
      _currentPlayer = currentPlayer;
      _status = StatusOfGame.INPROGRESS;
      _currentPhase = currentPhase;
    }

    protected int GetNextPlayer(IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      int next = 0;
      int inc = 1;
      do
      {
        next = (_currentPlayer + inc) % _players.Count;
        inc++;
      } while (!playersInfo[_players[next].PlayerColor].IsAlive);
      return next;
    }

    protected bool IsEndOfSetUp()
    {
      foreach (var info in _playersInfo)
      {
        if (info.Value.FreeUnits > 0)
        {
          return false;
        }
      }
      return true;
    }

    public static IDictionary<ArmyColor, Game.PlayerInfo> GetPlayersInfoClone(IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      Dictionary<ArmyColor, Game.PlayerInfo> playersInfoClone = new Dictionary<ArmyColor, Game.PlayerInfo>();
      foreach (var info in playersInfo)
      {
        playersInfoClone.Add(info.Key, (Game.PlayerInfo)info.Value.Clone());
      }
      return playersInfoClone;
    }

    public abstract IList<State> GetAllPossibilities();

    public int Simulate()
    {
      var game = new GameSimulation((GameBoard)_gameBoard.Clone(), _players, GetPlayersInfoClone(_playersInfo), _currentPhase, _currentPlayer);
      game.StartGame();
      for (int i = 0; i < _players.Count; ++i)
      {
        if (((IAI)_players[i]).IsWinner)
        {
          _status = (StatusOfGame)i;

          return i;
        }
      }
      return -1;
    }
  }
}