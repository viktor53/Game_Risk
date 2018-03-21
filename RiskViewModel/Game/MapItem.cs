namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Base class for map items.
  /// </summary>
  public abstract class MapItem : ViewModelBase
  {
    /// <summary>
    /// Coordinate X.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Coordinate Y.
    /// </summary>
    public int Y { get; set; }
  }
}