using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.Cards;

namespace Risk.AI
{
  internal class GameSimulation : IGame
  {
    private GameBoard _gameBoard;

    private IList<IPlayer> _players;

    private IDictionary<ArmyColor, Game.PlayerInfo> _playersInfo;

    private IPlayer _currentPlayer;

    private Attack _capturing;

    private bool _isFromStart;

    private Phase _currentPhase;

    public GameSimulation(bool isClassic)
    {
      _players = new List<IPlayer>();
      _playersInfo = new Dictionary<ArmyColor, Game.PlayerInfo>();
      _gameBoard = Game.GameSettings.GetGameBoard(isClassic);
      _isFromStart = true;
      _currentPhase = Phase.SETUP;
    }

    public GameSimulation(GameBoard board, IList<IPlayer> players, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Phase currentPhase, IPlayer currentPlayer)
    {
      _gameBoard = board;
      _players = players;
      _playersInfo = playersInfo;
      _isFromStart = true;
      _currentPhase = currentPhase;
      _currentPlayer = currentPlayer;
    }

    public bool AddPlayer(IPlayer player)
    {
      if (_players.Count < 6)
      {
        _players.Add(player);
        return true;
      }
      return false;
    }

    public GamePlanInfo GetGamePlan()
    {
      return new GamePlanInfo(_gameBoard.Connections, _gameBoard.Areas);
    }

    public void StartGame()
    {
      if (_players.Count >= 3)
      {
        SetUpPlayers();

        StartAllPlayers().Wait();

        Play();

        EndAllPlayer();
      }
    }

    private void Play()
    {
      if (_isFromStart)
      {
        PlaySetUpPhase();

        PlayToTheEnd();
      }
      else
      {
        if (_currentPhase == Phase.SETUP)
        {
          PlaySetUpPhaseFromThePoint();

          PlayToTheEnd();
        }
        else
        {
          PlayToTheEndFromThePoint();
        }
      }
    }

    /// <summary>
    /// Plays set up phase while there are free units.
    /// </summary>
    private void PlaySetUpPhase()
    {
      int units = Game.GameSettings.GetStartNumberFreeUnit(_players.Count);

      foreach (var player in _players)
      {
        player.FreeUnit = units;
      }

      for (int i = 0; i < units; ++i)
      {
        foreach (var player in _players)
        {
          _currentPlayer = player;
          player.PlaySetUp();
        }
      }
    }

    /// <summary>
    /// Play SetUpPhase of already started game.
    /// </summary>
    private void PlaySetUpPhaseFromThePoint()
    {
      int index = _players.IndexOf(_currentPlayer);

      for (int i = index; i < _players.Count; ++i)
      {
        _players[i].PlaySetUp();
      }

      IPlayer last = _players[_players.Count - 1];

      while (_playersInfo[last.PlayerColor].FreeUnits > 0)
      {
        foreach (var player in _players)
        {
          _currentPlayer = player;
          player.PlaySetUp();
        }
      }
    }

    /// <summary>
    /// Plays while nobody won.
    /// </summary>
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

            player.PlayDraft();

            player.PlayAttack();

            if (_playersInfo[player.PlayerColor].GetsCard)
            {
              RiskCard card = _gameBoard.GetCard();
              _playersInfo[player.PlayerColor].Cards.Add(card);
              player.AddCard(card);

              _playersInfo[player.PlayerColor].GetsCard = false;
            }

            if (IsWinner()) break;

            player.PlayFortify();
          }
        }
      }
    }

    /// <summary>
    /// Play already started game while nobody won.
    /// </summary>
    private void PlayToTheEndFromThePoint()
    {
      int index = _players.IndexOf(_currentPlayer);

      int phase = (int)_currentPhase;
      while (phase % 4 != 0 && !IsWinner())
      {
        switch ((Phase)phase)
        {
          case Phase.DRAFT:
            _currentPlayer.PlayDraft();
            break;

          case Phase.ATTACK:
            _currentPlayer.PlayAttack();

            if (_playersInfo[_currentPlayer.PlayerColor].GetsCard)
            {
              RiskCard card = _gameBoard.GetCard();
              _playersInfo[_currentPlayer.PlayerColor].Cards.Add(card);
              _currentPlayer.AddCard(card);

              _playersInfo[_currentPlayer.PlayerColor].GetsCard = false;
            }
            break;

          case Phase.FORTIFY:
            _currentPlayer.PlayFortify();
            break;

          default:
            break;
        }

        phase++;
      }

      for (int i = index + 1; i < _players.Count; ++i)
      {
        if (_playersInfo[_players[i].PlayerColor].IsAlive)
        {
          _players[i].PlayDraft();

          _players[i].PlayAttack();

          if (IsWinner())
          {
            return;
          }

          _players[i].PlayFortify();
        }
      }

      PlayToTheEnd();
    }

    /// <summary>
    /// Counts number of free units for the player.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <returns>number of free units</returns>
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

    /// <summary>
    /// Finds out if the player controls the region.
    /// </summary>
    /// <param name="regionID">id of region</param>
    /// <param name="playerColor">color of player</param>
    /// <returns>if the player controls the region</returns>
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

    /// <summary>
    /// Sets Up all players.
    /// </summary>
    public void SetUpPlayers()
    {
      if (_isFromStart)
      {
        SetUpPlayerInfo();

        SetUpPlayersOrder();
      }
      else
      {
        foreach (var player in _players)
        {
          player.FreeUnit = _playersInfo[player.PlayerColor].FreeUnits;
          foreach (var card in _playersInfo[player.PlayerColor].Cards)
          {
            player.AddCard(card);
          }
        }
      }
    }

    /// <summary>
    /// Initializes players information.
    /// </summary>
    private void SetUpPlayerInfo()
    {
      _playersInfo = new Dictionary<ArmyColor, Game.PlayerInfo>();
      int numberFreeUnits = Game.GameSettings.GetStartNumberFreeUnit(_players.Count);

      foreach (var player in _players)
      {
        _playersInfo.Add(player.PlayerColor, new Game.PlayerInfo(player.PlayerColor, numberFreeUnits));
      }
    }

    /// <summary>
    /// Randomly sets up players order.
    /// </summary>
    private void SetUpPlayersOrder()
    {
      List<IPlayer> orderedPlayers = new List<IPlayer>();
      while (_players.Count != 0)
      {
        Dictionary<IPlayer, int> playersRoll = new Dictionary<IPlayer, int>();
        foreach (var player in _players)
        {
          playersRoll.Add(player, _gameBoard.Dice.RollDice(1)[0]);
        }
        IPlayer max = GetMax(playersRoll);
        if (max != null)
        {
          orderedPlayers.Add(max);
          _players.Remove(max);
        }
      }
      _players = orderedPlayers;
    }

    /// <summary>
    /// Gets player with maximum roll.
    /// </summary>
    /// <param name="playersRoll">players and their rolls</param>
    /// <returns>player with maximum roll or null if does not exist</returns>
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

    /// <summary>
    /// Starts all players.
    /// </summary>
    /// <returns></returns>
    private async Task StartAllPlayers()
    {
      foreach (var player in _players)
      {
        await player.StartPlayer(this);
      }
    }

    /// <summary>
    /// Makes game move setup.
    /// </summary>
    /// <param name="move">setup move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, NotEnoughFreeUnit, NotYourArea, AlreadySetUpThisTurn)
    /// </returns>
    public MoveResult MakeMove(SetUp move)
    {
      if (_gameBoard.Areas[move.AreaID].ArmyColor != move.PlayerColor)
      {
        _playersInfo[move.PlayerColor].CapturedAreas++;
      }

      _gameBoard.Areas[move.AreaID].SizeOfArmy++;
      _gameBoard.Areas[move.AreaID].ArmyColor = move.PlayerColor;
      _playersInfo[move.PlayerColor].FreeUnits--;
      _currentPlayer.FreeUnit--;

      return MoveResult.OK;
    }

    /// <summary>
    /// Makes game move draft.
    /// </summary>
    /// <param name="move">draft move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, NotEnoughFreeUnit, NotYourArea)
    /// </returns>
    public MoveResult MakeMove(Draft move)
    {
      _gameBoard.Areas[move.AreaID].SizeOfArmy += move.NumberOfUnit;
      _playersInfo[move.PlayerColor].FreeUnits -= move.NumberOfUnit;
      _currentPlayer.FreeUnit -= move.NumberOfUnit;

      return MoveResult.OK;
    }

    /// <summary>
    /// Makes game move exchange card.
    /// </summary>
    /// <param name="move">exchange card move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, InvalidCombination)
    /// </returns>
    public MoveResult MakeMove(ExchangeCard move)
    {
      int units = _gameBoard.GetUnitPerCombination();
      _playersInfo[move.PlayerColor].FreeUnits += units;
      _currentPlayer.FreeUnit += units;

      //foreach (var card in move.Combination)
      //{
      //  if (card.TypeUnit != UnitType.Mix)
      //  {
      //    int id = ((NormalCard)card).Area;
      //    if (_gameBoard.Areas[id].ArmyColor == move.PlayerColor)
      //    {
      //      _gameBoard.Areas[id].SizeOfArmy += 2;

      //      break;
      //    }
      //  }
      //}

      RemoveCards(move.Combination);

      return MoveResult.OK;
    }

    /// <summary>
    /// Makes game move attack.
    /// </summary>
    /// <param name="move">attack move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, InvalidAttack, InvalidAttackerOrDefender, EmptyCapturedArea)
    /// </returns>
    public MoveResult MakeMove(Attack move)
    {
      CountLosts(move);

      return FindOutResult(move);
    }

    /// <summary>
    /// Makes game move capture.
    /// </summary>
    /// <param name="move">capture move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, NoCapturedArea, InvalidNumberUnit)
    /// </returns>
    public MoveResult MakeMove(Capture move)
    {
      _gameBoard.Areas[_capturing.AttackerAreaID].SizeOfArmy -= move.ArmyToMove;
      _gameBoard.Areas[_capturing.DefenderAreaID].SizeOfArmy += move.ArmyToMove;

      _playersInfo[move.PlayerColor].GetsCard = true;

      return MoveResult.OK;
    }

    /// <summary>
    /// Makes game move fortify.
    /// </summary>
    /// <param name="move">fortify move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, AlreadyFortifyThisTurn, NotConnected, InvalidNumberUnit)
    /// </returns>
    public MoveResult MakeMove(Fortify move)
    {
      _gameBoard.Areas[move.FromAreaID].SizeOfArmy -= move.SizeOfArmy;
      _gameBoard.Areas[move.ToAreaID].SizeOfArmy += move.SizeOfArmy;

      return MoveResult.OK;
    }

    /// <summary>
    /// Counts losts of both sides and subtracts losses.
    /// </summary>
    /// <param name="move">attack move command</param>
    private void CountLosts(Attack move)
    {
      int[] attRoll = _gameBoard.Dice.RollDice((int)move.AttackSize);
      int[] defRoll = _gameBoard.Areas[move.DefenderAreaID].SizeOfArmy >= 2 ? _gameBoard.Dice.RollDice(2) : _gameBoard.Dice.RollDice(1);

      int attDied = 0;
      int defDied = 0;
      for (int i = 0; i < Math.Min(attRoll.Length, defRoll.Length); ++i)
      {
        if (attRoll[i] > defRoll[i])
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
    }

    /// <summary>
    /// Finds out result of move. If area is captured or player won.
    /// </summary>
    /// <param name="move">attack move command</param>
    /// <returns>move result</returns>
    private MoveResult FindOutResult(Attack move)
    {
      if (_gameBoard.Areas[move.DefenderAreaID].SizeOfArmy == 0)
      {
        ArmyColor defColor = _gameBoard.Areas[move.DefenderAreaID].ArmyColor;

        _playersInfo[move.PlayerColor].CapturedAreas++;
        _playersInfo[defColor].CapturedAreas--;

        _gameBoard.Areas[move.DefenderAreaID].ArmyColor = move.PlayerColor;
        _capturing = move;

        _playersInfo[defColor].IsAlive = IsDefenderAlive(defColor);

        if (!_playersInfo[defColor].IsAlive)
        {
          foreach (var card in _playersInfo[defColor].Cards)
          {
            _playersInfo[move.PlayerColor].Cards.Add(card);
            _currentPlayer.AddCard(card);
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

    /// <summary>
    /// Checks if defender is alive.
    /// </summary>
    /// <param name="defenderColor">color of defender</param>
    /// <returns>if defender is alive</returns>
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

    /// <summary>
    /// Checks if player won.
    /// </summary>
    /// <returns>if player won</returns>
    private bool IsWinner()
    {
      if (_playersInfo[_currentPlayer.PlayerColor].CapturedAreas == _gameBoard.Areas.Length)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    /// <summary>
    /// Removes cards from player and returns them into package.
    /// </summary>
    /// <param name="combination">risk card combination</param>
    private void RemoveCards(IList<RiskCard> combination)
    {
      foreach (var card in combination)
      {
        _gameBoard.ReturnCard(card);
        _playersInfo[_currentPlayer.PlayerColor].Cards.Remove(card);
        _currentPlayer.RemoveCard(card);
      }
    }

    /// <summary>
    /// Ends all players.
    /// </summary>
    private void EndAllPlayer()
    {
      foreach (var player in _players)
      {
        player.EndPlayer(_playersInfo[player.PlayerColor].IsAlive);
      }
    }
  }
}