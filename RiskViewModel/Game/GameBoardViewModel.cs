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

namespace Risk.ViewModel.Game
{
  public class GameBoardViewModel: ViewModelBase, IGameBoardViewModel
  {
    private IWindowManager _windowManager;

    private ViewModelBase _gameDialog;

    private bool _isEnabled;

    private readonly string[] _phases = { "DRAFT", "ATTACK", "FORTIFY"};

    private readonly string[] _turns = { "YOUR TURN", "ENEMIES TURN" };

    private string _currentPhase;

    private string _turn;

    private int _numberCards;

    private int _freeArmy;

    private string _bg;

    private List<Planet> _planets;

    private IList<IList<bool>> _connections;

    public ICommand Planet_Click { get; private set; }

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

    public string CurrentPhase
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

    public string Turn
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

    public string BG {
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

    public BindingList<MapItem> MapItems { get; private set; }

    public GameBoardViewModel(IWindowManager windowManager)
    {
      _windowManager = windowManager;

      BG = Properties.Resources.bg1;

      CurrentPhase = _phases[0];

      Turn = _turns[0];

      NumberCards = 3;

      IsEnabled = false;

      FreeArmy = 30;

      GameDialogViewModel = new DraftViewModel(this);

      Planet_Click = new Command(PlanetClick);

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

      Model.Enums.Region prev = game.Areas[0].Reg;
      for (int i = 0; i < game.Areas.Length; ++i)
      {
        if(prev != game.Areas[i].Reg)
        {
          prev = game.Areas[i].Reg;
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
        bool correct = false;
        int a = 0;
        int b = 0;
        while(!correct)
        {
          a = X * ran.Next(Xm) / Xm + xa;
          b = Y * ran.Next(Ym) / Ym + ya;
          correct = IsCorrect(a, b, coords);
        }
        coords.Add(new Coordinates(a, b));
        ais.Add(new AreaInfo(a, b, game.Areas[i]));
      }

      GameBoardInfo gbi = new GameBoardInfo(game.Board, ais);

      _connections = gbi.Connections;
      LoadMap(gbi);
    }

    private int GetDistance(int x1, int y1, int x2, int y2)
    {
      return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    private bool IsCorrect(int x, int y, List<Coordinates> coords)
    {
      foreach(var co in coords)
      {
        if(GetDistance(x, y, co.X, co.Y) < 110)
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
        Planet p = new Planet(ai.X, ai.Y, Properties.Resources.planet1, ai.Area, Planet_Click);
        _planets.Insert(p.Area.ID, p);
      }

      for(int i = 0; i < gbi.Connections.Count; ++i)
      {
        for(int j = i; j < gbi.Connections[i].Count; ++j)
        {
          if(gbi.Connections[i][j])
          {
            MapItems.Add(new Connection(_planets[i].X + 70 / 2, _planets[i].Y + 70 / 2, _planets[j].X + 70 / 2, _planets[j].Y + 70 / 2));
          }
          
        }
      }

      foreach(var p in _planets)
      {
        MapItems.Add(p);
      }
    }

    private void PlanetClick()
    {
      System.Windows.Point p = Mouse.GetPosition(Application.Current.MainWindow);

      MessageBox.Show($"Points {p.X}, {p.Y}");

      Planet clicked = null;

      foreach (var pl in _planets)
      {
        if (GetDistance((int)p.X, (int)p.Y - 50, pl.X + 70 / 2, pl.Y + 70 / 2) <= 70)
        {
          clicked = pl;
          break;
        }
      }

      foreach(var pl in _planets)
      {
        pl.IsEnabled = false;
      }

      clicked.IsEnabled = true;

      for(int i = 0; i < _connections[clicked.Area.ID].Count; ++i)
      {
        if(_connections[clicked.Area.ID][i])
        {
          _planets[i].IsEnabled = true;
        }
      }

    }
  }

  public abstract class MapItem: ViewModelBase
  {
    public int X { get; set; }

    public int Y { get; set; }
  }

  public sealed class Planet: MapItem
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

  public sealed class Connection: MapItem
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

  class Coordinates
  {
    public int X { get; set; }

    public int Y { get; set; }

    public Coordinates(int x, int y)
    {
      X = x;
      Y = y;
    }
  }
}
