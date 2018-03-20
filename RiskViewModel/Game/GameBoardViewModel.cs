using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Data;
using Risk.Model.GamePlan;
using System.ComponentModel;
using Risk.Networking.Server;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using Risk.Model.Enums;
using Risk.Networking.Messages.Data;
using Risk.Networking.Client;
using System.Threading;
using Risk.ViewModel.Multiplayer;

namespace Risk.ViewModel.Game
{
  public class GameBoardViewModel : ViewModelBase, IGameBoardViewModel
  {
    private IWindowManager _windowManager;

    private ViewModelBase _gameDialog;

    private RiskClient _client;

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

    public ICommand Planet_Click { get; private set; }

    public ICommand Next_Click { get; private set; }

    public ICommand Cards_Click { get; private set; }

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
          _client.OnMoveResult += OnMoveResultNextPhase;
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

    public BindingList<MapItem> MapItems { get; private set; }

    public GameBoardViewModel(IWindowManager windowManager, RiskClient client, GameBoardInfo boardInfo)
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

    private void OnUpdate(object sender, EventArgs ev)
    {
      Area a = ((UpdateGameEventArgs)ev).Data;
      _planets[a.ID].SizeOfArmy = a.SizeOfArmy;
      _planets[a.ID].ArmyColor = a.ArmyColor;
    }

    private void OnFreeUnit(object sender, EventArgs ev)
    {
      FreeArmy = (int)((FreeUnitEventArgs)ev).Data;
    }

    private void OnArmyColor(object sender, EventArgs ev)
    {
      long color = ((ArmyColorEventArgs)ev).Data;
      PlayerColor = (ArmyColor)color;
    }

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

    private void OnMoveResultCard(object sender, EventArgs ev)
    {
      MoveResult mr = (MoveResult)((MoveResultEventArgs)ev).Data;
      if (mr == MoveResult.InvalidCombination)
      {
        GameDialogViewModel = new ErrorViewModel(this, $"No combination of card.");
      }
      _client.OnMoveResult += OnMoveResultNextPhase;
    }

    private void OnEndGame(object sender, EventArgs ev)
    {
      ViewModelBase viewModel = new MultiplayerViewModel(_windowManager, _client);
      IsEnabled = true;
      GameDialogViewModel = new WinnerViewModel(_windowManager, viewModel, ((EndGameEventArgs)ev).Data);
    }

    private int GetDistance(int x1, int y1, int x2, int y2)
    {
      return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

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

    private void PlanetClick()
    {
      System.Windows.Point p = Mouse.GetPosition(Application.Current.MainWindow);

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

    private void NextClick()
    {
      if (CurrentPhase != Phase.SETUP)
      {
        _client.SendNextPhaseAsync();
      }
    }

    private void CardsClick()
    {
      if (CurrentPhase == Phase.DRAFT)
      {
        _client.OnMoveResult += OnMoveResultCard;
        _client.SendExchangeCardAsync(PlayerColor);
      }
    }

    private void SetUpClick(Planet planet)
    {
      _client.SendSetUpMoveAsync(planet.ID, PlayerColor);
    }

    private void DraftClick(Planet planet)
    {
      if (!IsEnemy(planet))
      {
        Selected1 = planet;
        GameDialogViewModel = new DraftViewModel(this, _client);
      }
    }

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

    private bool IsEnemy(Planet planet)
    {
      return planet.ArmyColor != _playerColor;
    }

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

    private void EnableAllPlanet(bool enable)
    {
      foreach (var pl in _planets)
      {
        pl.IsEnabled = enable;
      }
    }

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