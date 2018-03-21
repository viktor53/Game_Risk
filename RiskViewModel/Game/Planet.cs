using System.Windows.Input;
using Risk.Model.Enums;
using Risk.Model.GamePlan;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents one planet on game board.
  /// </summary>
  public sealed class Planet : MapItem
  {
    private bool _isEnabled;

    private int _id;

    private int _sizeOfArmy;

    private ArmyColor _armyColor;

    /// <summary>
    /// Source of planet image.
    /// </summary>
    public string IMG { get; set; }

    /// <summary>
    /// Click on Planet. Executes action.
    /// </summary>
    public ICommand Planet_Click { get; private set; }

    /// <summary>
    /// If planet is enabled.
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
    /// ID of planet corresponding to area ID.
    /// </summary>
    public int ID => _id;

    /// <summary>
    /// Number of units on planet.
    /// </summary>
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

    /// <summary>
    /// Color of amry on planet.
    /// </summary>
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

    /// <summary>
    /// Initilizes planet with position on game board, its image and its area.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="img">source of planet image</param>
    /// <param name="area">area</param>
    /// <param name="click">command click on planet</param>
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