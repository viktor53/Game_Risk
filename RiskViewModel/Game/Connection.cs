namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents connection line between two planets.
  /// </summary>
  public sealed class Connection : MapItem
  {
    /// <summary>
    /// Coordinate X of end point.
    /// </summary>
    public int X2 { get; set; }

    /// <summary>
    /// Coordinate Y of end point.
    /// </summary>
    public int Y2 { get; set; }

    /// <summary>
    /// Initializes connection line with start point and end point.
    /// </summary>
    /// <param name="x">X coordinate of start point</param>
    /// <param name="y">Y coordinate of start point</param>
    /// <param name="x2">X coordinate of end point</param>
    /// <param name="y2">Y coordinate of end point</param>
    public Connection(int x, int y, int x2, int y2)
    {
      X = x;
      Y = y;
      X2 = x2;
      Y2 = y2;
    }
  }
}