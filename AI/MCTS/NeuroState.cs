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
  internal class NeuroState : State
  {
    private NeuroHeuristic _heuristic;

    public NeuroState(GameBoard gameBoard, IList<IPlayer> players,
      IDictionary<ArmyColor, Game.PlayerInfo> playersInfo,
      int currentPlayer, Phase currentPhase, NeuroHeuristic heuristic) : base(gameBoard, players, playersInfo, currentPlayer, currentPhase)
    {
      _heuristic = heuristic;
    }

    public override IList<State> GetAllPossibilities()
    {
      IList<State> states = null;
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

    private IList<State> GetAllSetUpPos()
    {
      IList<State> possibilities = new List<State>();

      IList<SetUp> movesPos = _heuristic.GetSetUpPossibilities(_players[_currentPlayer].PlayerColor, _gameBoard.Areas, _gameBoard.Connections);

      for (int i = 0; i < movesPos.Count; ++i)
      {
        var gameBoardCopy = (GameBoard)_gameBoard.Clone();
        var playersInfoCopy = GetPlayersInfoClone(_playersInfo);

        MoveManager.MakeMove(movesPos[i], gameBoardCopy, playersInfoCopy);

        var s = new NeuroState(gameBoardCopy, _players, playersInfoCopy, (_currentPlayer + 1) % _players.Count, _currentPhase, _heuristic);

        var moves = new Moves();
        moves.SetUpMove = movesPos[i];
        s.Moves = moves;

        possibilities.Add(s);
      }

      return possibilities;
    }

    private IList<State> GetAllPosOfTurn(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      List<State> possibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

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

          possibilities.AddRange(GetDraftMove(gameBoardCopy, playersInfoCopy, movesCopy));
        }
      }

      possibilities.AddRange(GetDraftMove(gameBoard, playersInfo, moves));

      return possibilities;
    }

    private IList<State> GetDraftMove(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> possibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      if (playersInfo[playerColor].FreeUnits > 0)
      {
        IList<Draft> movesPos = _heuristic.GetDraftPossibilities(playerColor, playersInfo[playerColor].FreeUnits, gameBoard.Areas, gameBoard.Connections);

        for (int i = 1; i < movesPos.Count; ++i)
        {
          var gameBoardCopy = (GameBoard)gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          var movesCopy = new Moves(moves);

          MoveManager.MakeMove(movesPos[i], gameBoardCopy, playersInfoCopy);

          movesCopy.DraftMoves.Add(movesPos[i]);

          possibilities.AddRange(GetDraftMove(gameBoardCopy, playersInfoCopy, movesCopy));
        }

        MoveManager.MakeMove(movesPos[0], gameBoard, playersInfo);

        moves.DraftMoves.Add(movesPos[0]);

        possibilities.AddRange(GetDraftMove(gameBoard, playersInfo, moves));
      }
      else
      {
        possibilities.AddRange(GetAttackMove(Helper.WhoCanAttack(gameBoard.Areas, gameBoard.Connections, playerColor), 0, gameBoard, playersInfo, moves));
      }

      return possibilities;
    }

    private IList<State> GetAttackMove(IList<Area> canAttack, int index, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> possibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      if (index < canAttack.Count)
      {
        Area attacker = canAttack[index];

        IList<Area> canBeAttacked = Helper.WhoCanBeAttacked(attacker, gameBoard.Areas, gameBoard.Connections, playerColor);

        for (int i = 0; i < canBeAttacked.Count; ++i)
        {
          var gameBoardCopy = (GameBoard)gameBoard.Clone();
          var playersInfoCopy = GetPlayersInfoClone(playersInfo);
          var movesCopy = new Moves(moves);

          int attackSize = _heuristic.GetAttackSize(playerColor, attacker, canBeAttacked[i], gameBoardCopy.Areas, gameBoardCopy.Connections);

          while (attackSize > 0)
          {
            int defID = canBeAttacked[i].ID;

            Attack move = new Attack(playerColor, attacker.ID, defID, (AttackSize)attackSize);
            movesCopy.AttackMoves.Add(move);

            MoveResult result = MoveManager.MakeMove(move, gameBoardCopy, playersInfoCopy);

            if (result == MoveResult.AreaCaptured)
            {
              int armiesToMove = _heuristic.GetNumberOfArmiesToMove(playerColor, gameBoardCopy.Areas[attacker.ID],
              gameBoardCopy.Areas[defID], attackSize, gameBoardCopy.Areas, gameBoardCopy.Connections);

              Capture cap = new Capture(playerColor, armiesToMove);

              MoveManager.MakeMove(cap, move, gameBoardCopy, playersInfoCopy);
              movesCopy.CaptureMoves.Add(cap);

              if (Helper.CanAttack(gameBoardCopy.Areas[defID], gameBoardCopy.Areas, gameBoardCopy.Connections, playerColor))
              {
                canAttack.Add(gameBoardCopy.Areas[defID]);
              }

              canAttack[index] = gameBoardCopy.Areas[attacker.ID];
              possibilities.AddRange(GetAttackMove(canAttack, index + 1, gameBoardCopy, playersInfoCopy, movesCopy));
              canAttack[index] = attacker;
              canAttack.Remove(gameBoardCopy.Areas[defID]);

              break;
            }
            else if (result == MoveResult.Winner)
            {
              HeuristicState state = new HeuristicState(null, null, null, (_currentPlayer + 1) % _players.Count, Phase.DRAFT);
              state.Status = (StatusOfGame)_currentPlayer;
              state.Moves = movesCopy;
              possibilities.Add(state);

              break;
            }

            attackSize = _heuristic.GetAttackSize(playerColor, gameBoardCopy.Areas[attacker.ID],
              gameBoardCopy.Areas[defID], gameBoardCopy.Areas, gameBoardCopy.Connections);
          }

          if (attackSize <= 0)
          {
            canAttack[index] = gameBoardCopy.Areas[attacker.ID];
            possibilities.AddRange(GetAttackMove(canAttack, index + 1, gameBoardCopy, playersInfoCopy, movesCopy));
            canAttack[index] = attacker;
          }
        }

        possibilities.AddRange(GetAttackMove(canAttack, index + 1, gameBoard, playersInfo, moves));
      }
      else
      {
        possibilities.AddRange(GetFortifyMoves(gameBoard, playersInfo, moves));
      }

      return possibilities;
    }

    private IList<State> GetFortifyMoves(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> possibilities = new List<State>();

      int nextPlayer = GetNextPlayer(playersInfo);

      IList<Fortify> movesPos = _heuristic.GetFortifyPossibilities(_players[_currentPlayer].PlayerColor, gameBoard.Areas, gameBoard.Connections);

      for (int i = 0; i < movesPos.Count; ++i)
      {
        var gameBoardCopy = (GameBoard)gameBoard.Clone();
        var playersInfoCopy = GetPlayersInfoClone(playersInfo);
        var movesCopy = new Moves(moves);

        MoveManager.MakeMove(movesPos[i], gameBoardCopy);
        movesCopy.FortifyMove = movesPos[i];

        var state1 = new NeuroState(gameBoardCopy, _players, playersInfoCopy, nextPlayer, Phase.DRAFT, _heuristic);
        state1.Moves = movesCopy;

        possibilities.Add(state1);
      }

      var state = new NeuroState(gameBoard, _players, playersInfo, nextPlayer, Phase.DRAFT, _heuristic);
      state.Moves = moves;

      possibilities.Add(state);

      return possibilities;
    }
  }
}