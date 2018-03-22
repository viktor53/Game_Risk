using System;
using System.Collections.Generic;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using Risk.Model.GamePlan;
using Risk.Model.Enums;
using Risk.Networking.Messages.Data;
using Risk.Networking.Client;
using Risk.ViewModel.Multiplayer;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents game board view model.
  /// </summary>
  public class GameBoardViewModel : ViewModelBase, IGameBoardViewModel
  {
    private IWindowManager _windowManager;

    private ViewModelBase _gameDialog;

    private IPlayer _client;

    private const int _diameter = 70;

    private const int _radius = 70 / 2;

    private const int _heightOfPanel = 80;

    private bool _isEnabled;

    private Phase _currentPhase;

    private Turn _turn;

    private int _numberCards;

    private int _freeArmy;

    private ArmyColor _playerColor;

    private string _bg;

    private List<Planet> _planets;

    private IList<IList<bool>> _connections;

    private Planet _selected1;

    private Planet _selected2;

    private bool _firstClick = true;

    /// <summary>
    /// Click on Planet button or game board. Selects or unselects planet and starts action depending on phase of game.
    /// </summary>
    public ICommand Planet_Click { get; private set; }

    /// <summary>
    /// Click on Next button. Goes into next phase.
    /// </summary>
    public ICommand Next_Click { get; private set; }

    /// <summary>
    /// Click on Cards button. Tries to exchange cards if player has a combination.
    /// </summary>
    public ICommand Cards_Click { get; private set; }

    /// <summary>
    /// Game dialog view model.
    /// </summary>
    public ViewModelBase GameDialogViewModel
    {
      get
      {
        return _gameDialog;
      }
      set
      {
        _gameDialog = value;
        if (_gameDialog == null)
        {
          if (CurrentPhase == Phase.SETUP)
          {
            _client.OnMoveResult += OnMoveResultSetUp;
          }
          else
          {
            _client.OnMoveResult += OnMoveResultNextPhase;
          }
          switch (CurrentPhase)
          {
            case Phase.ATTACK:
              WhoCanAttack();
              break;

            default:
              EnableAllPlanet(true);
              break;
          }
        }
        OnPropertyChanged("GameDialogViewModel");
      }
    }

    /// <summary>
    /// If game board is enabled.
    /// </summary>
    public bool IsEnabled
    {
      get
      {
        return _isEnabled;
      }
      set
      {
        _isEnabled = value;
        OnPropertyChanged("IsEnabled");
      }
    }

    /// <summary>
    /// Current phase of game.
    /// </summary>
    public Phase CurrentPhase
    {
      get
      {
        return _currentPhase;
      }
      set
      {
        _currentPhase = value;
        OnPropertyChanged("CurrentPhase");
      }
    }

    /// <summary>
    /// Who plays now.
    /// </summary>
    public Turn Turn
    {
      get
      {
        return _turn;
      }
      set
      {
        _turn = value;
        OnPropertyChanged("Turn");
      }
    }

    /// <summary>
    /// Number of cards in player's hand.
    /// </summary>
    public int NumberCards
    {
      get
      {
        return _numberCards;
      }
      set
      {
        _numberCards = value;
        OnPropertyChanged("NumberCards");
      }
    }

    /// <summary>
    /// number of free units that can be placed on planet.
    /// </summary>
    public int FreeArmy
    {
      get
      {
        return _freeArmy;
      }
      set
      {
        _freeArmy = value;
        OnPropertyChanged("FreeArmy");
      }
    }

    /// <summary>
    /// Source of image backgraund.
    /// </summary>
    public string BG
    {
      get
      {
        return _bg;
      }
      set
      {
        _bg = value;
        OnPropertyChanged("BG");
      }
    }

    /// <summary>
    /// Color of player.
    /// </summary>
    public ArmyColor PlayerColor
    {
      get
      {
        return _playerColor;
      }
      set
      {
        _playerColor = value;
        OnPropertyChanged("PlayerColor");
      }
    }

    /// <summary>
    /// First selected planet.
    /// </summary>
    public Planet Selected1
    {
      get
      {
        return _selected1;
      }
      private set
      {
        _selected1 = value;
      }
    }

    /// <summary>
    /// Second selected planet.
    /// </summary>
    public Planet Selected2
    {
      get
      {
        return _selected2;
      }
      private set
      {
        _selected2 = value;
      }
    }

    /// <summary>
    /// List of map items. Items on game board.
    /// </summary>
    public BindingList<MapItem> MapItems { get; private set; }

    /// <summary>
    /// Initializes game board view model and sets game plan.
    /// </summary>
    /// <param name="windowManager">window manager</param>
    /// <param name="client">player manager that is allowed to make action</param>
    /// <param name="boardInfo">information about game board.</param>
    public GameBoardViewModel(IWindowManager windowManager, IPlayer client, GameBoardInfo boardInfo)
    {
      _windowManager = windowManager;
      _client = client;
      _client.OnUpdate += OnUpdate;
      _client.OnFreeUnit += OnFreeUnit;
      _client.OnArmyColor += OnArmyColor;
      _client.OnYourTurn += OnYourTurn;
      _client.OnMoveResult += OnMoveResultSetUp;
      _client.OnUpdateCard += OnUpdateCard;
      _client.OnEndGame += OnEndGame;
      _client.ListenToGameCommandsAsync();

      BG = Properties.Resources.gameBg;
      CurrentPhase = Phase.SETUP;
      Turn = Turn.ENEMY;
      NumberCards = 0;
      IsEnabled = false;
      FreeArmy = 0;

      Planet_Click = new Command(PlanetClick);
      Next_Click = new Command(NextClick);
      Cards_Click = new Command(CardsClick);

      LoadMap(boardInfo);
    }

    /// <summary>
    /// Method that is called when OnUpdate event is raised. Updates game board.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">UpdateGameEventArgs</param>
    private void OnUpdate(object sender, EventArgs ev)
    {
      Area a = ((UpdateGameEventArgs)ev).Data;
      _planets[a.ID].SizeOfArmy = a.SizeOfArmy;
      _planets[a.ID].ArmyColor = a.ArmyColor;
    }

    /// <summary>
    /// Method that is called when OnFreeUnit event is raised. Updates number of free units.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">FreeUnitEventArgs</param>
    private void OnFreeUnit(object sender, EventArgs ev)
    {
      FreeArmy = (int)((FreeUnitEventArgs)ev).Data;
    }

    /// <summary>
    /// Method that is called when OnArmyColor event is raised. Updates color of player.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">ArmyColorEventArgs</param>
    private void OnArmyColor(object sender, EventArgs ev)
    {
      long color = ((ArmyColorEventArgs)ev).Data;
      PlayerColor = (ArmyColor)color;
    }

    /// <summary>
    /// Method that is called when OnUpdateCard event is raised. Updates number of cards.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">UpdateCardEventArgs</param>
    private void OnUpdateCard(object sender, EventArgs ev)
    {
      bool isAdd = ((UpdateCardEventArgs)ev).Data;
      if (isAdd)
      {
        NumberCards++;
      }
      else
      {
        NumberCards--;
      }
    }

    /// <summary>
    /// Method that is called when OnYourTurn event is raised. Notifies player that he is playing now.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">ConfirmationEventArgs</param>
    private void OnYourTurn(object sender, EventArgs ev)
    {
      bool isSetUp = ((ConfirmationEventArgs)ev).Data;
      Turn = Turn.YOU;
      IsEnabled = true;
      if (isSetUp)
      {
        CurrentPhase = Phase.SETUP;
        _client.OnMoveResult += OnMoveResultSetUp;
      }
      else
      {
        CurrentPhase = Phase.DRAFT;
        _client.OnMoveResult += OnMoveResultNextPhase;
      }
    }

    /// <summary>
    /// Method that is called when OnMoveResult event is raised. Processes move result during SetUp phase.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">MoveResultEventArgs</param>
    private void OnMoveResultSetUp(object sender, EventArgs ev)
    {
      MoveResult mr = (MoveResult)((MoveResultEventArgs)ev).Data;
      if (mr == MoveResult.OK)
      {
        IsEnabled = false;
        FreeArmy--;
        Turn = Turn.ENEMY;
      }
      else
      {
        GameDialogViewModel = new ErrorViewModel(this, $"Move ends with error: {mr}");
      }
    }

    /// <summary>
    /// Method that is called when OnMoveResult event is raised. Processes move result of next phase command.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">MoveResultEventArgs</param>
    private void OnMoveResultNextPhase(object sender, EventArgs ev)
    {
      MoveResult mr = (MoveResult)((MoveResultEventArgs)ev).Data;
      if (mr == MoveResult.OK)
      {
        int i = ((int)CurrentPhase + 1) % 4;
        CurrentPhase = (Phase)i;
        if (CurrentPhase == Phase.SETUP)
        {
          IsEnabled = false;
          CurrentPhase = Phase.DRAFT;
          Turn = Turn.ENEMY;
        }
        switch (CurrentPhase)
        {
          case Phase.ATTACK:
            _firstClick = true;
            WhoCanAttack();
            break;

          case Phase.FORTIFY:
            EnableAllPlanet(true);
            _firstClick = true;
            break;

          default:
            break;
        }
      }
      else
      {
        GameDialogViewModel = new ErrorViewModel(this, $"Move ends with error: {mr}");
      }
    }

    /// <summary>
    /// Method that is called when OnMoveResult event is raised.Processes move result during card exchanging.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">MoveResultEventArgs</param>
    private void OnMoveResultCard(object sender, EventArgs ev)
    {
      MoveResult mr = (MoveResult)((MoveResultEventArgs)ev).Data;
      if (mr == MoveResult.InvalidCombination)
      {
        GameDialogViewModel = new ErrorViewModel(this, $"No combination of card.");
      }
      _client.OnMoveResult += OnMoveResultNextPhase;
    }

    /// <summary>
    /// Method that is called when OnEndGame event is raised. Ends the game and notifies if player won.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">EndGameEventArgs</param>
    private void OnEndGame(object sender, EventArgs ev)
    {
      if (_client is IClient)
      {
        ViewModelBase viewModel = new MultiplayerViewModel(_windowManager, (IClient)_client);
        IsEnabled = true;
        GameDialogViewModel = new WinnerViewModel(_windowManager, viewModel, ((EndGameEventArgs)ev).Data);
      }
    }

    /// <summary>
    /// Gets distance between two points.
    /// </summary>
    /// <param name="x1">X coordinate of point A</param>
    /// <param name="y1">Y coordinate of point A</param>
    /// <param name="x2">X coordinate of point B</param>
    /// <param name="y2">Y coordinate of point B</param>
    /// <returns>distance between two points</returns>
    private int GetDistance(int x1, int y1, int x2, int y2)
    {
      return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    /// <summary>
    /// Loads map. Processes game board information and creates map items.
    /// </summary>
    /// <param name="gbi">game board information</param>
    private void LoadMap(GameBoardInfo gbi)
    {
      _planets = new List<Planet>();
      MapItems = new BindingList<MapItem>();

      foreach (var ai in gbi.AreaInfos)
      {
        Planet p = new Planet(ai.Position.X, ai.Position.Y, GetPlanetIMG(ai.IMG), ai.Area, Planet_Click);
        _planets.Insert(p.ID, p);
      }

      _connections = gbi.Connections;

      for (int i = 0; i < gbi.Connections.Count; ++i)
      {
        for (int j = i; j < gbi.Connections[i].Count; ++j)
        {
          if (gbi.Connections[i][j])
          {
            MapItems.Add(new Connection(_planets[i].X + _radius, _planets[i].Y + _radius, _planets[j].X + _radius, _planets[j].Y + _radius));
          }
        }
      }

      foreach (var p in _planets)
      {
        MapItems.Add(p);
      }
    }

    /// <summary>
    /// Gets source of image planet depending on type of planet.
    /// </summary>
    /// <param name="img">type of planet</param>
    /// <returns>source of image planet</returns>
    private string GetPlanetIMG(int img)
    {
      switch (img)
      {
        case 1:
          return Properties.Resources.planet1;

        case 2:
          return Properties.Resources.planet2;

        case 3:
          return Properties.Resources.planet3;

        case 4:
          return Properties.Resources.planet4;

        case 5:
          return Properties.Resources.planet5;

        case 6:
          return Properties.Resources.planet6;

        case 7:
          return Properties.Resources.planet7;

        case 8:
          return Properties.Resources.planet8;

        default:
          return Properties.Resources.planet1;
      }
    }

    /// <summary>
    /// Processes click on planet depending on phase of game.
    /// </summary>
    private void PlanetClick()
    {
      Point p = Mouse.GetPosition(Application.Current.MainWindow);

      Planet clicked = GetClickedPlanet((int)p.X, (int)p.Y);

      if (clicked != null)
      {
        switch (CurrentPhase)
        {
          case Phase.SETUP:
            SetUpClick(clicked);
            break;

          case Phase.DRAFT:
            DraftClick(clicked);
            break;

          case Phase.ATTACK:
            AttackClick(clicked);
            break;

          case Phase.FORTIFY:
            FortifyClick(clicked);
            break;
        }
      }
      else
      {
        _firstClick = true;
        switch (CurrentPhase)
        {
          case Phase.DRAFT:
            EnableAllPlanet(true);
            break;

          case Phase.ATTACK:
            WhoCanAttack();
            break;

          case Phase.FORTIFY:
            EnableAllPlanet(true);
            break;

          default:
            break;
        }
      }
    }

    /// <summary>
    /// Goes into next phase of game.
    /// </summary>
    private async void NextClick()
    {
      if (CurrentPhase != Phase.SETUP)
      {
        await _client.SendNextPhaseAsync();
      }
    }

    /// <summary>
    /// Tries exchange cards.
    /// </summary>
    private async void CardsClick()
    {
      if (CurrentPhase == Phase.DRAFT)
      {
        _client.OnMoveResult += OnMoveResultCard;
        await _client.SendExchangeCardAsync(PlayerColor);
      }
    }

    /// <summary>
    /// Makes setup move.
    /// </summary>
    /// <param name="planet">planet, where player clicked<param>
    private async void SetUpClick(Planet planet)
    {
      if (IsCorrectSetUp(planet))
      {
        await _client.SendSetUpMoveAsync(PlayerColor, planet.ID);
      }
      else
      {
        GameDialogViewModel = new ErrorViewModel(this, "Invalid SetUp move. It is not your area or there is neutral area.");
      }
    }

    /// <summary>
    /// Finds out if it is correct setup move.
    /// </summary>
    /// <param name="planet">clicked planet</param>
    /// <returns>if it is correct</returns>
    private bool IsCorrectSetUp(Planet planet)
    {
      if (planet.ArmyColor == ArmyColor.Neutral)
      {
        return true;
      }
      if (planet.ArmyColor == PlayerColor && !IsThereNeutralPlanet())
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Finds out if there is neutral planet.
    /// </summary>
    /// <returns>if there is neutral planet</returns>
    private bool IsThereNeutralPlanet()
    {
      foreach (var planet in _planets)
      {
        if (planet.ArmyColor == ArmyColor.Neutral)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Makes draft move.
    /// </summary>
    /// <param name="planet">planet, where player clicked</param>
    private void DraftClick(Planet planet)
    {
      if (!IsEnemy(planet))
      {
        Selected1 = planet;
        GameDialogViewModel = new DraftViewModel(this, _client);
      }
    }

    /// <summary>
    /// Makes attack move and goes into settings of attack.
    /// </summary>
    /// <param name="planet">planet, where player clicked</param>
    private void AttackClick(Planet planet)
    {
      if (_firstClick && planet.SizeOfArmy > 1)
      {
        WhoCanBeAttacked(planet);

        planet.IsEnabled = true;

        Selected1 = planet;

        _firstClick = false;
      }
      else
      {
        if (planet != Selected1)
        {
          EnableAllPlanet(false);

          planet.IsEnabled = true;

          Selected1.IsEnabled = true;

          Selected2 = planet;

          _firstClick = true;

          GameDialogViewModel = new AttackViewModel(this, _client);
        }
      }
    }

    /// <summary>
    /// Enables all planet that can attack.
    /// </summary>
    private void WhoCanAttack()
    {
      EnableAllPlanet(false);

      foreach (var pl in _planets)
      {
        if (!IsEnemy(pl) && pl.SizeOfArmy > 1 && HasEnemy(pl))
        {
          pl.IsEnabled = true;
        }
      }
    }

    /// <summary>
    /// Enables all planet that can be attacked from the planet.
    /// </summary>
    /// <param name="planet">planet, where attack goes from</param>
    private void WhoCanBeAttacked(Planet planet)
    {
      EnableAllPlanet(false);

      for (int i = 0; i < _connections[planet.ID].Count; ++i)
      {
        if (_connections[planet.ID][i] && IsEnemy(_planets[i]))
        {
          _planets[i].IsEnabled = true;
        }
      }
    }

    /// <summary>
    /// Finds out if the planet is enemy or not.
    /// </summary>
    /// <param name="planet">the planet</param>
    /// <returns>if the planet is enemy</returns>
    private bool IsEnemy(Planet planet)
    {
      return planet.ArmyColor != _playerColor;
    }

    /// <summary>
    /// Finds out if the planet has enemies around.
    /// </summary>
    /// <param name="planet">the planet</param>
    /// <returns>if the planet hase enemies</returns>
    private bool HasEnemy(Planet planet)
    {
      for (int i = 0; i < _connections[planet.ID].Count; ++i)
      {
        if (_connections[planet.ID][i] && IsEnemy(_planets[i]))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Makes fortify move and goes into settings of fortification.
    /// </summary>
    /// <param name="planet">planet, where player clicked</param>
    private void FortifyClick(Planet planet)
    {
      if (_firstClick)
      {
        if (!IsEnemy(planet) && planet.SizeOfArmy > 1)
        {
          Selected1 = planet;

          EnableConnectedPlanet(planet);

          planet.IsEnabled = true;

          _firstClick = false;
        }
      }
      else
      {
        Selected2 = planet;

        _firstClick = true;

        GameDialogViewModel = new FortifyViewModel(this, _client);
      }
    }

    /// <summary>
    /// Gets planet where player clicked.
    /// </summary>
    /// <param name="x">X coordinate of click</param>
    /// <param name="y">Y coordinate of click</param>
    /// <returns>the planet or null</returns>
    private Planet GetClickedPlanet(int x, int y)
    {
      foreach (var pl in _planets)
      {
        if (GetDistance(x, y - _heightOfPanel, pl.X + _radius, pl.Y + _radius) <= _diameter)
        {
          return pl;
        }
      }
      return null;
    }

    /// <summary>
    /// Enables or disables all planet depending on parametr.
    /// </summary>
    /// <param name="enable">enable or disable planet</param>
    private void EnableAllPlanet(bool enable)
    {
      foreach (var pl in _planets)
      {
        pl.IsEnabled = enable;
      }
    }

    /// <summary>
    /// Enables all planets connected with the planet through friendly planets.
    /// </summary>
    /// <param name="planet">the planet</param>
    private void EnableConnectedPlanet(Planet planet)
    {
      Stack<Planet> toVisit = new Stack<Planet>();
      HashSet<Planet> visited = new HashSet<Planet>();

      toVisit.Push(planet);
      visited.Add(planet);

      EnableAllPlanet(false);

      while (toVisit.Count != 0)
      {
        Planet p = toVisit.Pop();

        p.IsEnabled = true;

        for (int i = 0; i < _connections[p.ID].Count; ++i)
        {
          if (_connections[p.ID][i] && !IsEnemy(_planets[i]) && !visited.Contains(_planets[i]))
          {
            toVisit.Push(_planets[i]);
            visited.Add(_planets[i]);
          }
        }
      }
    }
  }
}