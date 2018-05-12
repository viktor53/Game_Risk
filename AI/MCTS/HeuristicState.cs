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
  internal class HeuristicState : State
  {
    public HeuristicState(GameBoard gameBoard, IList<IPlayer> players,
      IDictionary<ArmyColor, Game.PlayerInfo> playersInfo,
      int currentPlayer, Phase currentPhase) : base(gameBoard, players, playersInfo, currentPlayer, currentPhase)
    {
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

    /// <summary>
    /// Gets all possibilities of SetUp.
    /// </summary>
    /// <returns>all possible states</returns>
    private IList<State> GetAllSetUpPos()
    {
      IList<State> posibilities = new List<State>();

      IList<Area> nextMoves = Helper.GetUnoccupiedAreas(_gameBoard.Areas);

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      if (nextMoves.Count == 0)
      {
        nextMoves = HeuristicHelper.GetDraftPossibilities(_gameBoard.Areas, _gameBoard.Connections, playerColor);
      }

      foreach (var area in nextMoves)
      {
        SetUp move = new SetUp(playerColor, area.ID);

        var gameBoard = (GameBoard)_gameBoard.Clone();
        var playersInfo = GetPlayersInfoClone(_playersInfo);

        MoveManager.MakeMove(move, gameBoard, playersInfo);

        var s = new HeuristicState(gameBoard, _players, playersInfo, (_currentPlayer + 1) % _players.Count, _currentPhase);

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
    private IList<State> GetAllPosOfTurn(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      List<State> posibilities = new List<State>();

      ArmyColor playerColor = _players[_currentPlayer].PlayerColor;

      IList<Area> nextMoves = HeuristicHelper.GetDraftPossibilities(gameBoard.Areas, gameBoard.Connections, playerColor);

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

    private IList<State> GetDraftMove(IList<Area> canDraft, int index, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
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

          for (int i = 1; i < freeUnits; i += 3)
          {
            var gameBoardCopy2 = (GameBoard)gameBoard.Clone();
            var playersInfoCopy2 = GetPlayersInfoClone(playersInfo);
            var movesCopy2 = new Moves(moves);

            Draft move2 = new Draft(playerColor, canDraft[index].ID, i);
            MoveManager.MakeMove(move2, gameBoardCopy2, playersInfoCopy2);
            movesCopy2.DraftMoves.Add(move2);

            posibilities.AddRange(GetDraftMove(canDraft, index + 1, gameBoardCopy2, playersInfoCopy2, movesCopy2));
          }

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

    private IList<State> GetAttackMove(IList<Area> canAttack, int index, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
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

            Attack move = new Attack(playerColor, attacker.ID, defID, (AttackSize)maxAttack);
            movesCopy.AttackMoves.Add(move);

            MoveResult result = MoveManager.MakeMove(move, gameBoardCopy, playersInfoCopy);

            if (result == MoveResult.AreaCaptured)
            {
              Capture cap = new Capture(playerColor, gameBoardCopy.Areas[attacker.ID].SizeOfArmy - 1);
              MoveManager.MakeMove(cap, move, gameBoardCopy, playersInfoCopy);
              movesCopy.CaptureMoves.Add(cap);

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
              HeuristicState state = new HeuristicState(null, null, null, (_currentPlayer + 1) % _players.Count, Phase.DRAFT);
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

    private IList<State> GetFortifyMoves(IList<Area> canFortify, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Moves moves)
    {
      List<State> posibilities = new List<State>();

      int nextPlayer = GetNextPlayer(playersInfo);

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

          HeuristicState state1 = new HeuristicState(gameBoardCopy, _players, playersInfoCopy, nextPlayer, Phase.DRAFT);
          state1.Moves = movesCopy;
          posibilities.Add(state1);
        }
      }

      HeuristicState state = new HeuristicState(gameBoard, _players, playersInfo, nextPlayer, Phase.DRAFT);
      state.Moves = moves;
      posibilities.Add(state);

      return posibilities;
    }
  }
}