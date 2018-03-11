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
          _client.OnMoveResult += OnMoveResult;
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

    public GameBoardViewModel(IWindowManager windowManager, RiskClient client, GameBoardInfo boardInfo, SynchronizationContext ui)
    {
      _windowManager = windowManager;
      _client = client;
      _client.OnUpdate += OnUpdate;
      _client.OnFreeUnit += OnFreeUnit;
      _client.OnArmyColor += OnArmyColor;
      _client.OnYourTurn += OnYourTurn;
      _client.OnMoveResult += OnMoveResult;
      _client.ListenToGameCommands();

      BG = Properties.Resources.bg1;

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

    private void OnYourTurn(object sender, EventArgs ev)
    {
      bool isSetUp = ((ConfirmationEventArgs)ev).Data;
      Turn = Turn.YOU;
      IsEnabled = true;
      if (isSetUp)
      {
        CurrentPhase = Phase.SETUP;
      }
      else
      {
        CurrentPhase = Phase.DRAFT;
      }
    }

    private void OnMoveResult(object sender, EventArgs ev)
    {
      MoveResult mr = (MoveResult)((MoveResultEventArgs)ev).Data;
      if (mr == MoveResult.OK)
      {
        if (CurrentPhase == Phase.SETUP)
        {
          IsEnabled = false;
          FreeArmy--;
        }
        else
        {
          CurrentPhase = (Phase)(((int)CurrentPhase + 1) % 4);
          if (CurrentPhase == Phase.SETUP)
          {
            IsEnabled = false;
            CurrentPhase = Phase.DRAFT;
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
      }
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
        Planet p = new Planet(ai.Position.X, ai.Position.Y, Properties.Resources.planet1, ai.Area, Planet_Click);
        _planets.Insert(p.ID, p);
      }

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
        _client.SendNextPhase();
      }
    }

    private void CardsClick()
    {
      if (CurrentPhase == Phase.DRAFT)
      {
      }
    }

    private void SetUpClick(Planet planet)
    {
      _client.SendSetUpMove(planet.ID, PlayerColor);
    }

    private void DraftClick(Planet planet)
    {
      if (!IsEnemy(planet))
      {
        Selected1 = planet;
        _client.OnMoveResult -= OnMoveResult;
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

          _client.OnMoveResult -= OnMoveResult;

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

        _client.OnMoveResult -= OnMoveResult;

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

  public abstract class MapItem : ViewModelBase
  {
    public int X { get; set; }

    public int Y { get; set; }
  }

  public sealed class Planet : MapItem
  {
    private bool _isEnabled;

    private int _id;

    private int _sizeOfArmy;

    private ArmyColor _armyColor;

    public string IMG { get; set; }

    public ICommand Planet_Click { get; private set; }

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

    public int ID => _id;

    public int SizeOfArmy
    {
      get
      {
        return _sizeOfArmy;
      }
      set
      {
        _sizeOfArmy = value;
        OnPropertyChanged("SizeOfArmy");
      }
    }

    public ArmyColor ArmyColor
    {
      get
      {
        return _armyColor;
      }
      set
      {
        _armyColor = value;
        OnPropertyChanged("ArmyColor");
      }
    }

    public Planet(int x, int y, string img, Area area, ICommand click)
    {
      X = x;
      Y = y;
      IMG = img;
      SizeOfArmy = area.SizeOfArmy;
      ArmyColor = area.ArmyColor;
      _id = area.ID;
      IsEnabled = true;
      Planet_Click = click;
    }
  }

  public sealed class Connection : MapItem
  {
    public int X2 { get; set; }

    public int Y2 { get; set; }

    public Connection(int x, int y, int x2, int y2)
    {
      X = x;
      Y = y;
      X2 = x2;
      Y2 = y2;
    }
  }
}