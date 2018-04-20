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
using System.Diagnostics;

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

    public GameSimulation(int numberOfAreas)
    {
      _players = new List<IPlayer>();
      _playersInfo = new Dictionary<ArmyColor, Game.PlayerInfo>();
      _gameBoard = Game.GameSettings.GetGameBoard(numberOfAreas);
      _isFromStart = true;
      _currentPhase = Phase.SETUP;
    }

    public GameSimulation(GameBoard board, IList<IPlayer> players, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, Phase currentPhase, int currentPlayer)
    {
      _gameBoard = board;
      _players = players;
      _playersInfo = playersInfo;
      _currentPhase = currentPhase;
      _currentPlayer = _players[currentPlayer];
      _isFromStart = false;
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

    public GameBoard GetCurrentStateOfGameBoard()
    {
      return (GameBoard)_gameBoard.Clone();
    }

    public IDictionary<ArmyColor, Game.PlayerInfo> GetPlayersInfo()
    {
      Dictionary<ArmyColor, Game.PlayerInfo> playersInfo = new Dictionary<ArmyColor, Game.PlayerInfo>();
      foreach (var info in _playersInfo)
      {
        playersInfo.Add(info.Key, (Game.PlayerInfo)info.Value.Clone());
      }
      return playersInfo;
    }

    public IList<ArmyColor> GetOrderOfPlayers()
    {
      List<ArmyColor> orderOfPlayers = new List<ArmyColor>();
      foreach (var player in _players)
      {
        orderOfPlayers.Add(player.PlayerColor);
      }
      return orderOfPlayers;
    }

    public int GetCurrentPlayer()
    {
      return _players.IndexOf(_currentPlayer);
    }

    public int GetNumberOfCombination()
    {
      return _gameBoard.Combination;
    }

    public int[] GetBonusForRegions()
    {
      return (int[])_gameBoard.ArmyForRegion.Clone();
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
      IPlayer last = _players[_players.Count - 1];

      if (_playersInfo[last.PlayerColor].FreeUnits > 0)
      {
        int index = _players.IndexOf(_currentPlayer);

        for (int i = index + 1; i < _players.Count; ++i)
        {
          _currentPlayer = _players[i];
          _currentPlayer.PlaySetUp();
        }

        while (_playersInfo[last.PlayerColor].FreeUnits > 0)
        {
          foreach (var player in _players)
          {
            _currentPlayer = player;
            _currentPlayer.PlaySetUp();
          }
        }
      }
    }

    private Stopwatch sw = new Stopwatch();

    /// <summary>
    /// Plays while nobody won.
    /// </summary>
    private void PlayToTheEnd()
    {
      while (!GameBoardHelper.IsWinner(_currentPlayer.PlayerColor, _gameBoard, _playersInfo))
      {
        sw.Start();
        foreach (var player in _players)
        {
          if (_playersInfo[player.PlayerColor].IsAlive)
          {
            _currentPlayer = player;

            SetNumberFreeUnit(player);

            _currentPhase = Phase.DRAFT;
            player.PlayDraft();

            _currentPhase = Phase.ATTACK;
            player.PlayAttack();

            if (GameBoardHelper.IsWinner(_currentPlayer.PlayerColor, _gameBoard, _playersInfo))
            {
              break;
            }

            _currentPhase = Phase.FORTIFY;
            player.PlayFortify();
          }
        }

        sw.Stop();
        if (sw.Elapsed.TotalSeconds > 1)
        {
          return;
        }
      }
    }

    /// <summary>
    /// Play already started game while nobody won.
    /// </summary>
    private void PlayToTheEndFromThePoint()
    {
      int index = _players.IndexOf(_currentPlayer);

      //int p = (int)_currentPhase;
      //while (p % 4 != 0)
      //{
      //  switch ((Phase)p)
      //  {
      //    case Phase.DRAFT:
      //      _currentPhase = Phase.DRAFT;
      //      _players[index].PlayDraft();
      //      break;

      //    case Phase.ATTACK:
      //      _currentPhase = Phase.ATTACK;
      //      _players[index].PlayAttack();

      //      if (IsWinner())
      //      {
      //        return;
      //      }
      //      break;

      //    case Phase.FORTIFY:
      //      _currentPhase = Phase.FORTIFY;
      //      _players[index].PlayFortify();
      //      break;
      //  }

      //  p++;
      //}

      for (int i = index + 1; i < _players.Count; ++i)
      {
        if (_playersInfo[_players[i].PlayerColor].IsAlive)
        {
          _currentPlayer = _players[i];
          SetNumberFreeUnit(_players[i]);

          _currentPhase = Phase.DRAFT;
          _players[i].PlayDraft();

          _currentPhase = Phase.ATTACK;
          _players[i].PlayAttack();

          if (GameBoardHelper.IsWinner(_currentPlayer.PlayerColor, _gameBoard, _playersInfo))
          {
            return;
          }

          _currentPhase = Phase.FORTIFY;
          _players[i].PlayFortify();
        }
      }

      PlayToTheEnd();
    }

    /// <summary>
    /// Sets number of free units on start draft phase.
    /// </summary>
    /// <param name="player">player</param>
    public void SetNumberFreeUnit(IPlayer player)
    {
      int numberFreeUnit = GameBoardHelper.GetNumberFreeUnit(player.PlayerColor, _gameBoard);
      _playersInfo[player.PlayerColor].FreeUnits = numberFreeUnit;
      player.FreeUnit = numberFreeUnit;
    }

    /// <summary>
    /// Sets Up all players.
    /// </summary>
    public void SetUpPlayers()
    {
      if (_isFromStart)
      {
        SetUpPlayerInfo();

        _players = GameBoardHelper.SetUpPlayersOrder(_players, _gameBoard);
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
      _playersInfo[move.PlayerColor].GetsCard = false;

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

      if (!_playersInfo[_currentPlayer.PlayerColor].GetsCard)
      {
        RiskCard card = _gameBoard.GetCard();
        _playersInfo[_currentPlayer.PlayerColor].Cards.Add(card);
        _currentPlayer.AddCard(card);

        _playersInfo[_currentPlayer.PlayerColor].GetsCard = true;
      }

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

      bool a = _gameBoard.Areas[move.AttackerAreaID].SizeOfArmy <= 0;
      bool d = _gameBoard.Areas[move.DefenderAreaID].SizeOfArmy < 0;

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

        _playersInfo[defColor].IsAlive = GameBoardHelper.IsDefenderAlive(defColor, _playersInfo);

        if (!_playersInfo[defColor].IsAlive)
        {
          foreach (var card in _playersInfo[defColor].Cards)
          {
            _playersInfo[move.PlayerColor].Cards.Add(card);
            _currentPlayer.AddCard(card);
          }

          _playersInfo[defColor].Cards.Clear();
        }

        if (GameBoardHelper.IsWinner(move.PlayerColor, _gameBoard, _playersInfo))
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
        player.EndPlayer(_playersInfo[player.PlayerColor].IsAlive).Wait();
      }
    }
  }
}