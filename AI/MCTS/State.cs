using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal class State
  {
    private GameBoard _gameBoard;

    private IList<IPlayer> _players;

    private IDictionary<ArmyColor, Game.PlayerInfo> _playersInfo;

    private int _currentPlayer;

    private Phase _currentPhase;

    private StatusOfGame _status;

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

    public List<State> GetAllPosibilities()
    {
      List<State> states = null;
      if (_currentPhase == Phase.SETUP && !IsEndOfSetUp())
      {
        states = GetAllSetUpPos();
      }
      else
      {
        if (_gameBoard != null)
        {
          var gameBoardCopy = (GameBoard)_gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(_playersInfo);
          ArmyColor playerColor = _players[_currentPlayer].PlayerColor;
          playersInfoCopy[playerColor].FreeUnits = GameBoardHelper.GetNumberFreeUnit(playerColor, gameBoardCopy);

          states = GetAllPosOfTurn(gameBoardCopy, playersInfoCopy);
        }
      }

      _gameBoard = null;
      _playersInfo = null;

      return states;
    }

    /// <summary>
    /// Gets all possibilities of SetUp.
    /// </summary>
    /// <returns>all possible states</returns>
    private List<State> GetAllSetUpPos()
    {
      List<State> posibilities = new List<State>();

      IList<Area> nextMoves = Helper.GetUnoccupiedAreas(_gameBoard.Areas);

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      if (nextMoves.Count == 0)
      {
        nextMoves = HeuristicHelper.GetDraftPosibilities(_gameBoard.Areas, _gameBoard.Connections, playerColor);
      }

      foreach (var area in nextMoves)
      {
        SetUp move = new SetUp(playerColor, area.ID);

        var gameBoard = (GameBoard)_gameBoard.Clone();
        var playersInfo = GetPlayersInfoClone(_playersInfo);

        MoveManager.MakeMove(move, gameBoard, playersInfo);

        var s = new State(gameBoard, _players, playersInfo, (_currentPlayer + 1) % _players.Count, _currentPhase);

        Moves moves = new Moves();
        moves.SetUpMove = move;
        s.Moves = moves;

        posibilities.Add(s);
      }

      return posibilities;
    }

    /// <summary>
    /// Gets all possibilities of one turn (draft, attack, fortify).
    /// </summary>
    /// <returns>all possible state</returns>
    private List<State> GetAllPosOfTurn(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      List<State> posibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      IList<Area> nextMoves = HeuristicHelper.GetDraftPosibilities(gameBoard.Areas, gameBoard.Connections, playerColor);

      Moves moves = new Moves();

      while (playersInfo[playerColor].Cards.Count >= 5)
      {
        IList<RiskCard> combination = Helper.GetCombination(playersInfo[playerColor].Cards);

        ExchangeCard move = new ExchangeCard(playerColor, combination);
        MoveManager.MakeMove(move, gameBoard, playersInfo);
        moves.CardMoves.Add(move);
      }

      if (playersInfo[playerColor].Cards.Count >= 3)
      {
        IList<RiskCard> combination = Helper.GetCombination(playersInfo[playerColor].Cards);
        if (combination.Count != 0)
        {
          var gameBoardCopy = (GameBoard)gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          var movesCopy = new Moves(moves);

          ExchangeCard move = new ExchangeCard(playerColor, combination);
          MoveManager.MakeMove(move, gameBoardCopy, playersInfoCopy);
          movesCopy.CardMoves.Add(move);

          posibilities.AddRange(GetDraftMove(nextMoves, 0, gameBoardCopy, playersInfoCopy, movesCopy));
        }
      }

      posibilities.AddRange(GetDraftMove(nextMoves, 0, gameBoard, playersInfo, moves));

      return posibilities;
    }

    private List<State> GetDraftMove(IList<Area> canDraft, int index, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> posibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;
      int freeUnits = playersInfo[playerColor].FreeUnits;

      if (freeUnits > 0)
      {
        if (index == canDraft.Count - 1)
        {
          Draft move = new Draft(playerColor, canDraft[index].ID, freeUnits);
          MoveManager.MakeMove(move, gameBoard, playersInfo);
          moves.DraftMoves.Add(move);

          posibilities.AddRange(GetDraftMove(canDraft, index + 1, gameBoard, playersInfo, moves));
        }
        else
        {
          var gameBoardCopy = (GameBoard)gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          var movesCopy = new Moves(moves);

          Draft move = new Draft(playerColor, canDraft[index].ID, freeUnits);
          MoveManager.MakeMove(move, gameBoardCopy, playersInfoCopy);
          movesCopy.DraftMoves.Add(move);

          posibilities.AddRange(GetDraftMove(canDraft, index + 1, gameBoardCopy, playersInfoCopy, movesCopy));

          //for (int i = 1; i <= freeUnits; ++i)
          //{
          //  var gameBoardCopy = (GameBoard)gameBoard.Clone();
          //  var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          //  var movesCopy = new Moves(moves);

          //  Draft move = new Draft(playerColor, canDraft[index].ID, i);
          //  MoveManager.MakeMove(move, gameBoardCopy, playersInfoCopy);
          //  movesCopy.DraftMoves.Add(move);

          //  posibilities.AddRange(GetDraftMove(canDraft, index + 1, gameBoardCopy, playersInfoCopy, movesCopy));
          //}

          posibilities.AddRange(GetDraftMove(canDraft, index + 1, gameBoard, playersInfo, moves));
        }
      }
      else
      {
        IList<Area> canAttack = HeuristicHelper.WhoCanAttack(gameBoard.Areas, gameBoard.Connections, playerColor);
        posibilities.AddRange(GetAttackMove(canAttack, 0, gameBoard, playersInfo, moves));
      }

      return posibilities;
    }

    private List<State> GetAttackMove(IList<Area> canAttack, int index, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> posibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      if (index < canAttack.Count)
      {
        Area attacker = canAttack[index];
        int maxAttack = Helper.GetMaxSizeOfAttack(attacker);
        IList<Area> canBeAttack = HeuristicHelper.WhereCanAttack(canAttack[index], gameBoard.Areas, gameBoard.Connections);

        for (int i = 0; i < canBeAttack.Count; ++i)
        {
          var gameBoardCopy = (GameBoard)gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          var movesCopy = new Moves(moves);

          while (maxAttack > 0)
          {
            int defID = canBeAttack[i].ID;

            Attack move1 = new Attack(playerColor, attacker.ID, defID, (AttackSize)maxAttack);
            movesCopy.AttackMoves.Add(move1);

            MoveResult result = MoveManager.MakeMove(move1, gameBoardCopy, playersInfoCopy);

            if (result == MoveResult.AreaCaptured)
            {
              Capture cap2 = new Capture(playerColor, gameBoardCopy.Areas[attacker.ID].SizeOfArmy - 1);
              MoveManager.MakeMove(cap2, move1, gameBoardCopy, playersInfoCopy);
              movesCopy.CaptureMoves.Add(cap2);

              if (gameBoardCopy.Areas[canBeAttack[i].ID].SizeOfArmy > 1)
              {
                for (int j = 0; j < gameBoardCopy.Connections[defID].Length; ++j)
                {
                  if (gameBoardCopy.Connections[defID][j] && HeuristicHelper.IsGoodAttack(gameBoardCopy.Areas[defID], gameBoardCopy.Areas[j]))
                  {
                    canAttack.Add(gameBoardCopy.Areas[defID]);
                    break;
                  }
                }
              }

              canAttack[index] = gameBoardCopy.Areas[attacker.ID];
              posibilities.AddRange(GetAttackMove(canAttack, index + 1, gameBoardCopy, playersInfoCopy, movesCopy));
              canAttack[index] = attacker;
              canAttack.Remove(gameBoardCopy.Areas[defID]);

              break;
            }
            else if (result == MoveResult.Winner)
            {
              State state = new State(null, null, null, (_currentPlayer + 1) % _players.Count, Phase.DRAFT);
              state.Status = (StatusOfGame)_currentPlayer;
              state.Moves = movesCopy;
              posibilities.Add(state);

              break;
            }

            maxAttack = Helper.GetMaxSizeOfAttack(gameBoardCopy.Areas[attacker.ID]);
          }

          if (maxAttack <= 0)
          {
            canAttack[index] = gameBoardCopy.Areas[attacker.ID];
            posibilities.AddRange(GetAttackMove(canAttack, index + 1, gameBoardCopy, playersInfoCopy, movesCopy));
            canAttack[index] = attacker;
          }
        }

        posibilities.AddRange(GetAttackMove(canAttack, index + 1, gameBoard, playersInfo, moves));
      }
      else
      {
        IList<Area> canFortify = HeuristicHelper.WhoCanFortify(gameBoard.Areas, gameBoard.Connections, playerColor);
        posibilities.AddRange(GetFortifyMoves(canFortify, gameBoard, playersInfo, moves));
      }

      return posibilities;
    }

    private List<State> GetFortifyMoves(IList<Area> canFortify, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> posibilities = new List<State>();

      int nextPlayer = GetNextPlayer(playersInfo);

      //StatusOfGame status = IsWinner(gameBoard);

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      for (int i = 0; i < canFortify.Count; ++i)
      {
        Area from = canFortify[i];
        IList<Area> where = HeuristicHelper.WhereCanFortify(from, gameBoard.Areas, gameBoard.Connections);
        for (int j = 0; j < where.Count; ++j)
        {
          var gameBoardCopy = (GameBoard)gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          var movesCopy = new Moves(moves);

          Fortify move = new Fortify(playerColor, from.ID, where[j].ID, from.SizeOfArmy - 1);
          MoveManager.MakeMove(move, gameBoardCopy);
          movesCopy.FortifyMove = move;

          State state1 = new State(gameBoardCopy, _players, playersInfoCopy, nextPlayer, Phase.DRAFT);
          state1.Moves = movesCopy;
          posibilities.Add(state1);
        }
      }

      State state = new State(gameBoard, _players, playersInfo, nextPlayer, Phase.DRAFT);
      state.Moves = moves;
      posibilities.Add(state);

      //if (status == StatusOfGame.INPROGRESS)
      //{
      //}
      //else
      //{
      //  State state = new State(gameBoard, _players, playersInfo, nextPlayer, Phase.DRAFT);
      //  state.Moves = moves;
      //  state.Status = status;
      //  posibilities.Add(state);
      //}

      return posibilities;
    }

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

    private bool IsEndOfSetUp()
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

    private StatusOfGame IsWinner(GameBoard gameBoard)
    {
      ArmyColor winner = gameBoard.Areas[0].ArmyColor;
      for (int i = 0; i < gameBoard.Areas.Length; ++i)
      {
        if (gameBoard.Areas[i].ArmyColor != winner)
        {
          return StatusOfGame.INPROGRESS;
        }
      }

      for (int i = 0; i < _players.Count; ++i)
      {
        if (_players[i].PlayerColor == winner)
        {
          return (StatusOfGame)i;
        }
      }

      return StatusOfGame.INPROGRESS;
    }

    private int GetNextPlayer(IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
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

    public static IDictionary<ArmyColor, Game.PlayerInfo> GetPlayersInfoClone(IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      Dictionary<ArmyColor, Game.PlayerInfo> playersInfoClone = new Dictionary<ArmyColor, Game.PlayerInfo>();
      foreach (var info in playersInfo)
      {
        playersInfoClone.Add(info.Key, (Game.PlayerInfo)info.Value.Clone());
      }
      return playersInfoClone;
    }
  }
}