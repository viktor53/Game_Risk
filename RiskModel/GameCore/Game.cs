﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Model.Cards;
using Risk.Model.Factories;

namespace Risk.Model.GameCore
{
  public class Game : IGame
  {
    //TODO Risk Cards add region counting
    private class GameSettings
    {
      public static GameBoard GetGameBoard(bool isClassic)
      {
        ClassicGameBoardFactory creator = new ClassicGameBoardFactory();
        return creator.CreateGameBoard();
      }

      public static int GetStartNumberFreeUnit(int numberPlayers)
      {
        switch (numberPlayers)
        {
          case 3:
            return 35;

          case 4:
            return 30;

          case 5:
            return 25;

          case 6:
            return 20;

          default:
            return -1;
        }
      }
    }

    private class PlayerInfo
    {
      public ArmyColor PlayerColor { get; private set; }

      public int FreeUnits { get; set; }

      public IList<RiskCard> Cards { get; private set; }

      public bool IsAlive { get; set; }

      public PlayerInfo(ArmyColor playerColor, int freeUnits)
      {
        PlayerColor = playerColor;
        FreeUnits = freeUnits;
        IsAlive = true;
      }
    }

    private GameBoard _gameBoard;

    private Phase _currentPhase;

    private IPlayer _currentPlayer;

    private bool _alreadySetUp;

    private bool _isAreaCaptured;

    private Attack _capturing;

    private IList<IPlayer> _players;

    private Dictionary<ArmyColor, PlayerInfo> _playersInfo;

    public Game(IList<IPlayer> players, bool isClassic)
    {
      _players = players;
      _gameBoard = GameSettings.GetGameBoard(isClassic);
      SetUpPlayerInfo();
    }

    public void StartGame()
    {
      SetUpPlayersOrder();

      PlaySetUpPhase();

      PlayToTheEnd();
    }

    private void PlaySetUpPhase()
    {
      _currentPhase = Phase.SETUP;

      foreach (var player in _players)
      {
        player.FreeUnit = GameSettings.GetStartNumberFreeUnit(_players.Count);
      }

      for (int i = 0; i < GameSettings.GetStartNumberFreeUnit(_players.Count); ++i)
      {
        foreach (var player in _players)
        {
          _currentPlayer = player;
          player.PlaySetUp();
        }
      }
    }

    private void PlayToTheEnd()
    {
      while (!IsWinner())
      {
        foreach (var player in _players)
        {
          if (_playersInfo[player.PlayerColor].IsAlive)
          {
            _currentPlayer = player;

            int numberFreeUnit = GetNumberFreeUnit(player.PlayerColor);
            _playersInfo[player.PlayerColor].FreeUnits = numberFreeUnit;
            player.FreeUnit = numberFreeUnit;

            RiskCard card = _gameBoard.GetCard();
            _playersInfo[player.PlayerColor].Cards.Add(card);
            player.Cards.Add(card);

            _currentPhase = Phase.DRAFT;
            player.PlayDraft();

            _currentPhase = Phase.ATTACK;
            player.PlayAttack();

            if (IsWinner()) break;

            _currentPhase = Phase.FORTIFY;
            player.PlayFortify();
          }
        }
      }
    }

    private bool IsWinner()
    {
      ArmyColor playerColor = _gameBoard.Areas[0].ArmyColor;
      foreach (var area in _gameBoard.Areas)
      {
        if (area.ArmyColor != playerColor)
        {
          return false;
        }
      }
      return true;
    }

    private int GetNumberFreeUnit(ArmyColor playerColor)
    {
      int occupiedAreas = 0;
      foreach (var area in _gameBoard.Areas)
      {
        if (area.ArmyColor == playerColor)
        {
          occupiedAreas++;
        }
      }

      int numberFreeUnits = occupiedAreas / 3;

      if (numberFreeUnits < 3)
      {
        numberFreeUnits = 3;
      }

      for (int i = 0; i < _gameBoard.ArmyForRegion.Length; ++i)
      {
        if (IsControlingRegion(i, playerColor))
        {
          numberFreeUnits += _gameBoard.ArmyForRegion[i];
        }
      }

      return numberFreeUnits;
    }

    private bool IsControlingRegion(int regionID, ArmyColor playerColor)
    {
      foreach (var area in _gameBoard.Areas)
      {
        if (area.RegionID == regionID && area.ArmyColor != playerColor)
        {
          return false;
        }
      }
      return true;
    }

    private void SetUpPlayersOrder()
    {
      List<IPlayer> orderedPlayers = new List<IPlayer>();
      while (_players.Count != 0)
      {
        Dictionary<IPlayer, int> playersRoll = new Dictionary<IPlayer, int>();
        foreach (var player in _players)
        {
          playersRoll.Add(player, _gameBoard.Dice.RollDice(1)[0]);
          IPlayer max = GetMax(playersRoll);
          if (max != null)
          {
            orderedPlayers.Add(max);
            _players.Remove(max);
          }
        }
      }
      _players = orderedPlayers;
    }

    private IPlayer GetMax(Dictionary<IPlayer, int> playersRoll)
    {
      int max = 0;
      IPlayer p = null;
      foreach (var pr in playersRoll)
      {
        if (max < pr.Value)
        {
          max = pr.Value;
          p = pr.Key;
        }
      }

      playersRoll.Remove(p);

      if (!playersRoll.ContainsValue(max))
      {
        return p;
      }
      else
      {
        return null;
      }
    }

    private void SetUpPlayerInfo()
    {
      _playersInfo = new Dictionary<ArmyColor, PlayerInfo>();
      int numberFreeUnits = GameSettings.GetStartNumberFreeUnit(_players.Count);
      foreach (var player in _players)
      {
        _playersInfo.Add(player.PlayerColor, new PlayerInfo(player.PlayerColor, numberFreeUnits));
      }
    }

    //TODO AreaID check
    public MoveResult MakeMove(Draft move)
    {
      if (IsCorrectMove(Phase.DRAFT, move.PlayerColor))
      {
        if (_gameBoard.Areas[move.AreaID].ArmyColor == move.PlayerColor)
        {
          if (_playersInfo[move.PlayerColor].FreeUnits >= move.NumberOfUnit)
          {
            _gameBoard.Areas[move.AreaID].SizeOfArmy += move.NumberOfUnit;
            _playersInfo[move.PlayerColor].FreeUnits -= move.NumberOfUnit;
            _currentPlayer.FreeUnit -= move.NumberOfUnit;

            return MoveResult.OK;
          }
          else
          {
            return MoveResult.NotEnoughFreeUnit;
          }
        }
        else
        {
          return MoveResult.NotYourArea;
        }
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    public MoveResult MakeMove(ExchangeCard move)
    {
      if (IsCorrectMove(Phase.DRAFT, move.PlayerColor))
      {
        if (_gameBoard.IsCorrectCombination(move.Combination))
        {
          if (HasCards(move.Combination))
          {
            int units = _gameBoard.GetUnitPerCombination();
            _playersInfo[move.PlayerColor].FreeUnits = units;
            _currentPlayer.FreeUnit = units;

            RemoveCards(move.Combination);

            return MoveResult.OK;
          }
        }
        return MoveResult.InvalidCombination;
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    public MoveResult MakeMove(Fortify move)
    {
      if (IsCorrectMove(Phase.FORTIFY, move.PlayerColor))
      {
        if (_gameBoard.IsConnected(move.FromAreaID, move.ToAreaID))
        {
          if (_gameBoard.Areas[move.FromAreaID].SizeOfArmy > move.SizeOfArmy)
          {
            _gameBoard.Areas[move.FromAreaID].SizeOfArmy -= move.SizeOfArmy;
            _gameBoard.Areas[move.ToAreaID].SizeOfArmy += move.SizeOfArmy;

            return MoveResult.OK;
          }
          else
          {
            return MoveResult.InvalidNumberUnit;
          }
        }
        else
        {
          return MoveResult.NotConnected;
        }
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    public MoveResult MakeMove(Attack move)
    {
      if (IsCorrectMove(Phase.ATTACK, move.PlayerColor))
      {
        if (!_isAreaCaptured)
        {
          if (_gameBoard.Areas[move.AttackerAreaID].ArmyColor == move.PlayerColor && _gameBoard.Areas[move.DefenderAreaID].ArmyColor != move.PlayerColor)
          {
            if (CanAttack(move))
            {
              return MakeAttack(move);
            }
            else
            {
              return MoveResult.InvalidAttack;
            }
          }
          else
          {
            return MoveResult.InvalidAttackerOrDefender;
          }
        }
        else
        {
          return MoveResult.EmptyCapturedArea;
        }
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    public MoveResult MakeMove(Capture move)
    {
      if (IsCorrectMove(Phase.ATTACK, move.PlayerColor))
      {
        if (_isAreaCaptured)
        {
          if (move.ArmyToMove < _gameBoard.Areas[_capturing.AttackerAreaID].SizeOfArmy && move.ArmyToMove >= (int)_capturing.AttackSize)
          {
            _gameBoard.Areas[_capturing.AttackerAreaID].SizeOfArmy -= move.ArmyToMove;
            _gameBoard.Areas[_capturing.DefenderAreaID].SizeOfArmy += move.ArmyToMove;

            return MoveResult.OK;
          }
          else
          {
            return MoveResult.InvalidNumberUnit;
          }
        }
        else
        {
          return MoveResult.NoCapturedArea;
        }
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    public MoveResult MakeMove(SetUp move)
    {
      if (IsCorrectMove(Phase.SETUP, move.PlayerColor))
      {
        if (!_alreadySetUp)
        {
          if (_gameBoard.Areas[move.AreaID].ArmyColor == ArmyColor.Neutral || _gameBoard.Areas[move.AreaID].ArmyColor == move.PlayerColor)
          {
            if (_playersInfo[move.PlayerColor].FreeUnits > 0)
            {
              _alreadySetUp = true;

              _gameBoard.Areas[move.AreaID].SizeOfArmy++;
              _playersInfo[move.PlayerColor].FreeUnits--;

              return MoveResult.OK;
            }
            else
            {
              return MoveResult.NotEnoughFreeUnit;
            }
          }
          else
          {
            return MoveResult.NotYourArea;
          }
        }
        else
        {
          return MoveResult.AlreadySetUpThisTurn;
        }
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    private bool IsCorrectMove(Phase phase, ArmyColor playerColor)
    {
      if (phase == _currentPhase && playerColor == _currentPlayer.PlayerColor)
      {
        return true;
      }
      return false;
    }

    private bool HasCards(IList<RiskCard> combination)
    {
      foreach (var card in combination)
      {
        if (!_playersInfo[_currentPlayer.PlayerColor].Cards.Contains(card))
        {
          return false;
        }
      }
      return true;
    }

    private void RemoveCards(IList<RiskCard> combination)
    {
      foreach (var card in combination)
      {
        _gameBoard.ReturnCard(card);
        _playersInfo[_currentPlayer.PlayerColor].Cards.Remove(card);
        _currentPlayer.Cards.Remove(card);
      }
    }

    private bool CanAttack(Attack move)
    {
      return _gameBoard.Board[move.AttackerAreaID][move.DefenderAreaID] && (int)move.AttackSize < _gameBoard.Areas[move.AttackerAreaID].SizeOfArmy;
    }

    private MoveResult MakeAttack(Attack move)
    {
      int[] attRoll = _gameBoard.Dice.RollDice((int)move.AttackSize);
      int[] defRoll = _gameBoard.Areas[move.DefenderAreaID].SizeOfArmy < 2 ? _gameBoard.Dice.RollDice(1) : _gameBoard.Dice.RollDice(2);

      int attDied = 0;
      int defDied = 0;
      for (int i = 0; i < Math.Min(attRoll.Length, defRoll.Length); ++i)
      {
        if (attRoll[i] > defRoll[0])
        {
          defDied++;
        }
        else
        {
          attDied++;
        }
      }

      _gameBoard.Areas[move.AttackerAreaID].SizeOfArmy -= attDied;
      _gameBoard.Areas[move.DefenderAreaID].SizeOfArmy -= defDied;

      if (_gameBoard.Areas[move.DefenderAreaID].SizeOfArmy == 0)
      {
        ArmyColor defColor = _gameBoard.Areas[move.DefenderAreaID].ArmyColor;

        _gameBoard.Areas[move.DefenderAreaID].ArmyColor = move.PlayerColor;
        _isAreaCaptured = true;
        _capturing = move;

        _playersInfo[defColor].IsAlive = IsDefenderAlive(defColor);
        if (!_playersInfo[defColor].IsAlive)
        {
          foreach (var card in _playersInfo[defColor].Cards)
          {
            _playersInfo[move.PlayerColor].Cards.Add(card);
            _currentPlayer.Cards.Add(card);
          }

          _playersInfo[defColor].Cards.Clear();
        }

        if (IsWinner())
        {
          return MoveResult.Winner;
        }
        else
        {
          return MoveResult.AreaCaptured;
        }
      }

      return MoveResult.OK;
    }

    private bool IsDefenderAlive(ArmyColor defenderColor)
    {
      foreach (var area in _gameBoard.Areas)
      {
        if (area.ArmyColor == defenderColor)
        {
          return true;
        }
      }
      return false;
    }
  }
}