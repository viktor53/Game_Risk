using Risk.Model.Enums;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
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
}