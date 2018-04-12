using System;
using System.Collections.Generic;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Model.Cards;
using Risk.Model.Factories;
using log4net;
using System.Threading.Tasks;

namespace Risk.Model.GameCore
{
  public class Game : IGame
  {
    /// <summary>
    /// Represents settings of game. (Game board, free unit on start)
    /// </summary>
    public static class GameSettings
    {
      /// <summary>
      /// Creates new game board.
      /// </summary>
      /// <param name="isClassic">classic game board or random generated</param>
      /// <returns>new game board</returns>
      public static GameBoard GetGameBoard(bool isClassic)
      {
        IGameBoardFactory creator;
        if (isClassic)
        {
          creator = new ClassicGameBoardFactory();
        }
        else
        {
          creator = new RandomGameBoardFactory();
        }
        return creator.CreateGameBoard();
      }

      /// <summary>
      /// Gets number of free units on the start of game depending on number of players.
      /// </summary>
      /// <param name="numberPlayers">number of players</param>
      /// <returns>free units on the start</returns>
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

    /// <summary>
    /// Keeps control information about player so player can not cheat.
    /// </summary>
    public class PlayerInfo : ICloneable
    {
      /// <summary>
      /// Color of player.
      /// </summary>
      public ArmyColor PlayerColor { get; private set; }

      /// <summary>
      /// Number of free units in setup and draft phases.
      /// </summary>
      public int FreeUnits { get; set; }

      /// <summary>
      /// Player's risk cards.
      /// </summary>
      public IList<RiskCard> Cards { get; private set; }

      /// <summary>
      /// If player is still alive and plays.
      /// </summary>
      public bool IsAlive { get; set; }

      /// <summary>
      /// If player gets one card. If player captures area, he will get a card.
      /// </summary>
      public bool GetsCard { get; set; }

      /// <summary>
      /// Number of captured areas.
      /// </summary>
      public int CapturedAreas { get; set; }

      /// <summary>
      /// Creates player information.
      /// </summary>
      /// <param name="playerColor">color of player</param>
      /// <param name="freeUnits">free units</param>
      public PlayerInfo(ArmyColor playerColor, int freeUnits)
      {
        PlayerColor = playerColor;
        FreeUnits = freeUnits;
        IsAlive = true;
        GetsCard = false;
        Cards = new List<RiskCard>();
      }

      public object Clone()
      {
        PlayerInfo infoCopy = new PlayerInfo(PlayerColor, FreeUnits);
        infoCopy.IsAlive = IsAlive;
        infoCopy.GetsCard = GetsCard;
        infoCopy.CapturedAreas = CapturedAreas;
        foreach (var card in Cards)
        {
          infoCopy.Cards.Add(card);
        }

        return infoCopy;
      }
    }

    private readonly ILog _logger;

    private readonly GameBoard _gameBoard;

    private readonly int _capacity;

    private IList<IPlayer> _players;

    private Dictionary<ArmyColor, PlayerInfo> _playersInfo;

    private Phase _currentPhase;

    private IPlayer _currentPlayer;

    private bool _alreadySetUp;

    private bool _alreadyFortify;

    private bool _isAreaCaptured;

    private Attack _capturing;

    /// <summary>
    /// Creates game.
    /// </summary>
    /// <param name="isClassic">classic game board or generated</param>
    /// <param name="capacity">maximum of players</param>
    /// <param name="logger">logger</param>
    public Game(bool isClassic, int capacity, ILog logger)
    {
      _capacity = capacity < 3 ? 3 : capacity;

      _players = new List<IPlayer>();
      _gameBoard = GameSettings.GetGameBoard(isClassic);

      _logger = logger;
      _logger?.Info($"New game with capacity {capacity}, classic {isClassic}");
    }

    /// <summary>
    /// Adds new player into the game.
    /// </summary>
    /// <param name="player">new player</param>
    /// <returns>true if player is added, false if game is full</returns>
    public bool AddPlayer(IPlayer player)
    {
      if (_players.Count < _capacity)
      {
        _players.Add(player);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Gets game plan (areas, connections).
    /// </summary>
    /// <returns>game plan (areas, connections)</returns>
    public GamePlanInfo GetGamePlan()
    {
      Area[] areas = new Area[_gameBoard.Areas.Length];
      for (int i = 0; i < areas.Length; ++i)
      {
        Area origin = _gameBoard.Areas[i];
        Area copy = new Area(origin.ID, origin.RegionID);
        copy.SizeOfArmy = origin.SizeOfArmy;
        copy.ArmyColor = origin.ArmyColor;
        areas[i] = copy;
      }

      bool[][] connections = new bool[_gameBoard.Connections.Length][];
      connections = (bool[][])_gameBoard.Connections.Clone();

      return new GamePlanInfo(connections, areas);
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

    /// <summary>
    /// Starts game and plays until the end.
    /// </summary>
    public void StartGame()
    {
      if (_players.Count >= 3)
      {
        _logger?.Info("Start game");

        SetUpPlayerInfo();

        _players = GameBoardHelper.SetUpPlayersOrder(_players, _gameBoard);

        StartAllPlayer().Wait();

        PlaySetUpPhase();

        PlayToTheEnd();

        EndAllPlayer();

        _logger?.Info("End game");
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
      if (IsCorrectMove(Phase.SETUP, move.PlayerColor))
      {
        if (!_alreadySetUp)
        {
          if (IsCorrectSetUp(move))
          {
            if (_playersInfo[move.PlayerColor].FreeUnits > 0)
            {
              _alreadySetUp = true;

              if (_gameBoard.Areas[move.AreaID].ArmyColor != move.PlayerColor)
              {
                _playersInfo[move.PlayerColor].CapturedAreas++;
              }

              _gameBoard.Areas[move.AreaID].SizeOfArmy++;
              _gameBoard.Areas[move.AreaID].ArmyColor = move.PlayerColor;
              _playersInfo[move.PlayerColor].FreeUnits--;

              _logger?.Info($"Player={move.PlayerColor},Area={move.AreaID}");

              var r = UpdateAllPlayers(_gameBoard.Areas[move.AreaID]);
              r.Wait();

              return MoveResult.OK;
            }
            else
            {
              return MoveResult.NotEnoughFreeUnit;
            }
          }
          else
          {
            Console.WriteLine($"In game {move.PlayerColor}, {_gameBoard.Areas[move.AreaID].ArmyColor}");
            return MoveResult.InvalidSetUp;
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
      if (IsCorrectMove(Phase.DRAFT, move.PlayerColor))
      {
        if (_gameBoard.Areas[move.AreaID].ArmyColor == move.PlayerColor)
        {
          if (_playersInfo[move.PlayerColor].FreeUnits >= move.NumberOfUnit)
          {
            _gameBoard.Areas[move.AreaID].SizeOfArmy += move.NumberOfUnit;
            _playersInfo[move.PlayerColor].FreeUnits -= move.NumberOfUnit;
            _currentPlayer.FreeUnit -= move.NumberOfUnit;

            var r = UpdateAllPlayers(_gameBoard.Areas[move.AreaID]);
            r.Wait();

            _logger?.Info($"Player={move.PlayerColor},Area={move.AreaID},Units=+{move.NumberOfUnit}");

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
      if (IsCorrectMove(Phase.DRAFT, move.PlayerColor))
      {
        if (GameBoardHelper.IsCorrectCombination(move.Combination))
        {
          if (HasCards(move.Combination))
          {
            int units = _gameBoard.GetUnitPerCombination();
            _playersInfo[move.PlayerColor].FreeUnits += units;
            _currentPlayer.FreeUnit += units;

            foreach (var card in move.Combination)
            {
              if (card.TypeUnit != UnitType.Mix)
              {
                int id = ((NormalCard)card).Area;
                if (_gameBoard.Areas[id].ArmyColor == move.PlayerColor)
                {
                  _gameBoard.Areas[id].SizeOfArmy += 2;

                  var r = UpdateAllPlayers(_gameBoard.Areas[id]);
                  r.Wait();

                  _logger?.Info($"Player={move.PlayerColor},Area={id},Units=+2");

                  break;
                }
              }
            }

            _logger?.Info($"Player={move.PlayerColor},ExchangeCombination=({move.Combination[0].TypeUnit},{move.Combination[1].TypeUnit},{move.Combination[2].TypeUnit}),ExchangeResult{units}");

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
      if (IsCorrectMove(Phase.ATTACK, move.PlayerColor))
      {
        if (_isAreaCaptured)
        {
          if (move.ArmyToMove < _gameBoard.Areas[_capturing.AttackerAreaID].SizeOfArmy && move.ArmyToMove >= (int)_capturing.AttackSize)
          {
            _gameBoard.Areas[_capturing.AttackerAreaID].SizeOfArmy -= move.ArmyToMove;
            _gameBoard.Areas[_capturing.DefenderAreaID].SizeOfArmy += move.ArmyToMove;

            _logger?.Info($"Player={move.PlayerColor},AttackerArea={_capturing.AttackerAreaID},DefenderArea={_capturing.DefenderAreaID},Units={move.ArmyToMove}");

            var r1 = UpdateAllPlayers(_gameBoard.Areas[_capturing.AttackerAreaID]);
            var r2 = UpdateAllPlayers(_gameBoard.Areas[_capturing.DefenderAreaID]);

            Task.WaitAll(r1, r2);

            _isAreaCaptured = false;

            _playersInfo[move.PlayerColor].GetsCard = true;

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
      if (IsCorrectMove(Phase.FORTIFY, move.PlayerColor))
      {
        if (!_alreadyFortify)
        {
          if (GameBoardHelper.IsConnected(move.FromAreaID, move.ToAreaID, _gameBoard))
          {
            if (_gameBoard.Areas[move.FromAreaID].SizeOfArmy > move.SizeOfArmy)
            {
              _alreadyFortify = true;

              _gameBoard.Areas[move.FromAreaID].SizeOfArmy -= move.SizeOfArmy;
              _gameBoard.Areas[move.ToAreaID].SizeOfArmy += move.SizeOfArmy;

              _logger?.Info($"Player={move.PlayerColor},FromArea={move.FromAreaID},ToArea={move.ToAreaID},Units={move.SizeOfArmy}");

              var r1 = UpdateAllPlayers(_gameBoard.Areas[move.FromAreaID]);
              var r2 = UpdateAllPlayers(_gameBoard.Areas[move.ToAreaID]);

              Task.WaitAll(r1, r2);

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
          return MoveResult.AlreadyFortifyThisTurn;
        }
      }
      else
      {
        return move.PlayerColor != _currentPlayer.PlayerColor ? MoveResult.NotYourTurn : MoveResult.BadPhase;
      }
    }

    /// <summary>
    /// Checks if move is in correct phase and it is player's turn.
    /// </summary>
    /// <param name="phase">phase of move</param>
    /// <param name="playerColor">color of player</param>
    /// <returns>if it is correct</returns>
    private bool IsCorrectMove(Phase phase, ArmyColor playerColor)
    {
      if (phase == _currentPhase && playerColor == _currentPlayer.PlayerColor)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Finds out if the set up move is correct.
    /// </summary>
    /// <param name="move">setup move command</param>
    /// <returns>if it is correct</returns>
    private bool IsCorrectSetUp(SetUp move)
    {
      if (_gameBoard.Areas[move.AreaID].ArmyColor == ArmyColor.Neutral)
      {
        return true;
      }
      if (_gameBoard.Areas[move.AreaID].ArmyColor == move.PlayerColor && !GameBoardHelper.IsThereNeutralArea(_gameBoard, _playersInfo))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Checks if areas are connected and attacker has enough units.
    /// </summary>
    /// <param name="move">attack move command</param>
    /// <returns>if it is correct</returns>
    private bool CanAttack(Attack move)
    {
      return _gameBoard.Connections[move.AttackerAreaID][move.DefenderAreaID] && (int)move.AttackSize < _gameBoard.Areas[move.AttackerAreaID].SizeOfArmy;
    }

    /// <summary>
    /// Makes attack move.
    /// </summary>
    /// <param name="move">attack move command</param>
    /// <returns>results of move</returns>
    private MoveResult MakeAttack(Attack move)
    {
      CountLosts(move);

      MoveResult result = FindOutResult(move);

      var r1 = UpdateAllPlayers(_gameBoard.Areas[move.AttackerAreaID]);
      var r2 = UpdateAllPlayers(_gameBoard.Areas[move.DefenderAreaID]);

      Task.WaitAll(r1, r2);

      return result;
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

      _logger?.Info($"Player={move.PlayerColor},AttackerArea={move.AttackerAreaID},DefenderArea={move.DefenderAreaID},AttLost={attDied},DefLost={defDied}");
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
        _isAreaCaptured = true;
        _capturing = move;

        _playersInfo[defColor].IsAlive = GameBoardHelper.IsDefenderAlive(defColor, _playersInfo);

        if (!_playersInfo[defColor].IsAlive)
        {
          foreach (var card in _playersInfo[defColor].Cards)
          {
            _playersInfo[move.PlayerColor].Cards.Add(card);
            _currentPlayer.AddCard(card);

            _logger?.Info($"Player={move.PlayerColor},NewCard={card.TypeUnit}");
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
    /// Initializes players information.
    /// </summary>
    private void SetUpPlayerInfo()
    {
      _playersInfo = new Dictionary<ArmyColor, PlayerInfo>();
      int numberFreeUnits = GameSettings.GetStartNumberFreeUnit(_players.Count);

      _logger?.Info($"Starting number of unit {numberFreeUnits}");

      foreach (var player in _players)
      {
        _playersInfo.Add(player.PlayerColor, new PlayerInfo(player.PlayerColor, numberFreeUnits));
      }
    }

    /// <summary>
    /// Starts all players.
    /// </summary>
    private async Task StartAllPlayer()
    {
      foreach (var player in _players)
      {
        await player.StartPlayer(this);
      }
    }

    /// <summary>
    /// Ends all players.
    /// </summary>
    private void EndAllPlayer()
    {
      foreach (var player in _players)
      {
        _logger?.Info($"Player={player.PlayerColor},Winner={_playersInfo[player.PlayerColor].IsAlive}");

        player.EndPlayer(_playersInfo[player.PlayerColor].IsAlive);
      }
    }

    /// <summary>
    /// Plays set up phase while there are free units.
    /// </summary>
    private void PlaySetUpPhase()
    {
      _logger?.Info("SetUp phase");

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
          _alreadySetUp = false;
        }
      }
    }

    /// <summary>
    /// Plays while nobody won.
    /// </summary>
    private void PlayToTheEnd()
    {
      while (!GameBoardHelper.IsWinner(_currentPlayer.PlayerColor, _gameBoard, _playersInfo))
      {
        foreach (var player in _players)
        {
          if (_playersInfo[player.PlayerColor].IsAlive)
          {
            _currentPlayer = player;

            int numberFreeUnit = GameBoardHelper.GetNumberFreeUnit(player.PlayerColor, _gameBoard);
            _playersInfo[player.PlayerColor].FreeUnits = numberFreeUnit;
            player.FreeUnit = numberFreeUnit;

            _logger?.Info("Draft phase");
            _logger?.Info($"Player={player.PlayerColor},FreeUnit={numberFreeUnit}");

            _currentPhase = Phase.DRAFT;
            player.PlayDraft();

            _logger?.Info("Attack phase");

            _currentPhase = Phase.ATTACK;
            player.PlayAttack();

            if (_playersInfo[player.PlayerColor].GetsCard)
            {
              RiskCard card = _gameBoard.GetCard();
              _playersInfo[player.PlayerColor].Cards.Add(card);
              player.AddCard(card);

              _playersInfo[player.PlayerColor].GetsCard = false;

              _logger?.Info($"Player={player.PlayerColor},NewCard={card.TypeUnit}");
            }

            if (GameBoardHelper.IsWinner(player.PlayerColor, _gameBoard, _playersInfo)) break;

            _logger?.Info("Fortify phase");

            _currentPhase = Phase.FORTIFY;
            player.PlayFortify();
            _alreadyFortify = false;
          }
        }
      }
    }

    /// <summary>
    /// Checks if the player has cards.
    /// </summary>
    /// <param name="combination">risk cards combination</param>
    /// <returns>if the player has cards</returns>
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
    /// Updates game plan of all players.
    /// </summary>
    /// <param name="area">changed area</param>
    private async Task UpdateAllPlayers(Area area)
    {
      foreach (var player in _players)
      {
        await player.UpdateGame(area.ID, area.ArmyColor, area.SizeOfArmy);
      }
    }
  }
}