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

namespace Risk.ViewModel.Game
{
  public class GameBoardViewModel : ViewModelBase, IGameBoardViewModel
  {
    private IWindowManager _windowManager;

    private ViewModelBase _gameDialog;

    private const int _diameter = 70;

    private const int _radius = 70 / 2;

    private const int _heightOfPanel = 80;

    private bool _isEnabled;

    private Phase _currentPhase;

    private Turn _turn;

    private int _numberCards;

    private int _freeArmy;

    private readonly ArmyColor _playerColor;

    private string _bg;

    private List<Planet> _planets;

    private IList<IList<bool>> _connections;

    private Planet _selected1;

    private Planet _selected2;

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

    public GameBoardViewModel(IWindowManager windowManager)
    {
      _windowManager = windowManager;

      BG = Properties.Resources.bg1;

      CurrentPhase = Phase.DRAFT;

      Turn = Turn.YOU;

      NumberCards = 3;

      IsEnabled = false;

      FreeArmy = 30;

      GameDialogViewModel = new FortifyViewModel(this);

      _playerColor = ArmyColor.Blue;

      Planet_Click = new Command(PlanetClick);
      Next_Click = new Command(NextClick);
      Cards_Click = new Command(CardsClick);

      var temp = new Risk.Model.Factories.ClassicGameBoardFactory();
      var game = temp.CreateGameBoard();

      int X = (int)(Application.Current.MainWindow.ActualWidth - 100) / 3;
      int Y = (int)(Application.Current.MainWindow.ActualHeight - 200) / 2;
      int g = 3000;

      List<AreaInfo> ais = new List<AreaInfo>();

      int Ym = g;
      int Xm = (int)Math.Round((double)(g * X / Y));

      Random ran = new Random();
      bool mark = false;

      int xa = 0;
      int ya = 0;

      List<Coordinates> coords = new List<Coordinates>();

      int prevReg = game.Areas[0].RegionID;
      ArmyColor c = game.Areas[0].ArmyColor;
      for (int i = 0; i < game.Areas.Length; ++i)
      {
        if (prevReg != game.Areas[i].RegionID)
        {
          prevReg = game.Areas[i].RegionID;
          c++;
          if (mark)
          {
            xa = 0;
            ya += Y;
            mark = false;
          }
          else
          {
            xa += X;
            mark = xa >= 2 * X;
          }
        }
        game.Areas[i].ArmyColor = c;
        game.Areas[i].SizeOfArmy = 3;
        bool correct = false;
        int a = 0;
        int b = 0;
        while (!correct)
        {
          a = X * ran.Next(Xm) / Xm + xa;
          b = Y * ran.Next(Ym) / Ym + ya;
          correct = IsCorrect(a, b, coords);
        }
        var co = new Coordinates(a, b);
        coords.Add(co);
        ais.Add(new AreaInfo(co, game.Areas[i]));
      }

      GameBoardInfo gbi = new GameBoardInfo(game.Connections, ais);

      _connections = gbi.Connections;
      LoadMap(gbi);
    }

    private int GetDistance(int x1, int y1, int x2, int y2)
    {
      return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    private bool IsCorrect(int x, int y, List<Coordinates> coords)
    {
      foreach (var co in coords)
      {
        if (GetDistance(x, y, co.X, co.Y) < 110)
        {
          return false;
        }
      }
      return true;
    }

    private void LoadMap(GameBoardInfo gbi)
    {
      _planets = new List<Planet>();
      MapItems = new BindingList<MapItem>();

      foreach (var ai in gbi.AreaInfos)
      {
        Planet p = new Planet(ai.Position.X, ai.Position.Y, Properties.Resources.planet1, ai.Area, Planet_Click);
        _planets.Insert(p.Area.ID, p);
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

    private bool firstClick = true;

    private void PlanetClick()
    {
      System.Windows.Point p = Mouse.GetPosition(Application.Current.MainWindow);

      Planet clicked = GetClickedPlanet((int)p.X, (int)p.Y);

      if (clicked != null)
      {
        switch (CurrentPhase)
        {
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
        firstClick = true;
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
        }
      }
    }

    private void NextClick()
    {
      CurrentPhase += 1 % 3;

      switch (CurrentPhase)
      {
        case Phase.DRAFT:
          Turn = Turn.ENEMY;
          break;

        case Phase.ATTACK:
          firstClick = true;
          WhoCanAttack();
          break;

        case Phase.FORTIFY:
          EnableAllPlanet(true);
          firstClick = true;
          break;
      }
    }

    private void CardsClick()
    {
      if (CurrentPhase == Phase.DRAFT)
      {
      }
    }

    private void DraftClick(Planet planet)
    {
      if (!IsEnemy(planet))
      {
        Selected1 = planet;
        GameDialogViewModel = new DraftViewModel(this);
      }
    }

    private void AttackClick(Planet planet)
    {
      if (firstClick && planet.SizeOfArmy > 1)
      {
        WhoCanBeAttacked(planet);

        planet.IsEnabled = true;

        Selected1 = planet;

        firstClick = false;
      }
      else
      {
        if (planet != Selected1)
        {
          EnableAllPlanet(false);

          planet.IsEnabled = true;

          Selected1.IsEnabled = true;

          Selected2 = planet;

          firstClick = false;

          GameDialogViewModel = new AttackViewModel(this);
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

      for (int i = 0; i < _connections[planet.Area.ID].Count; ++i)
      {
        if (_connections[planet.Area.ID][i] && IsEnemy(_planets[i]))
        {
          _planets[i].IsEnabled = true;
        }
      }
    }

    private bool IsEnemy(Planet planet)
    {
      return planet.Area.ArmyColor != _playerColor;
    }

    private bool HasEnemy(Planet planet)
    {
      for (int i = 0; i < _connections[planet.Area.ID].Count; ++i)
      {
        if (_connections[planet.Area.ID][i] && IsEnemy(_planets[i]))
        {
          return true;
        }
      }
      return false;
    }

    private void FortifyClick(Planet planet)
    {
      if (firstClick)
      {
        if (!IsEnemy(planet) && planet.SizeOfArmy > 1)
        {
          Selected1 = planet;

          EnableConnectedPlanet(planet);

          planet.IsEnabled = true;

          firstClick = false;
        }
      }
      else
      {
        Selected2 = planet;

        GameDialogViewModel = new FortifyViewModel(this);

        firstClick = true;
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

        for (int i = 0; i < _connections[p.Area.ID].Count; ++i)
        {
          if (_connections[p.Area.ID][i] && !IsEnemy(_planets[i]) && !visited.Contains(_planets[i]))
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

    public string IMG { get; set; }

    public Area Area { get; private set; }

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

    public int SizeOfArmy
    {
      get
      {
        return Area.SizeOfArmy;
      }
      set
      {
        Area.SizeOfArmy = value;
        OnPropertyChanged("SizeOfArmy");
      }
    }

    public ArmyColor ArmyColor
    {
      get
      {
        return Area.ArmyColor;
      }
      set
      {
        Area.ArmyColor = value;
        OnPropertyChanged("ArmyColor");
      }
    }

    public Planet(int x, int y, string img, Area area, ICommand click)
    {
      X = x;
      Y = y;
      IMG = img;
      Area = area;
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