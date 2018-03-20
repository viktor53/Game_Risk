using Risk.Model.GamePlan;

namespace Risk.Networking.Messages.Data
{
  /// <summary>
  /// Message data containing information about the area.
  /// </summary>
  public sealed class AreaInfo
  {
    /// <summary>
    /// Position of area on game plan.
    /// </summary>
    public Coordinates Position { get; private set; }

    /// <summary>
    /// Area of the game board.
    /// </summary>
    public Area Area { get; private set; }

    /// <summary>
    /// Image that will be shown.
    /// </summary>
    public int IMG { get; private set; }

    /// <summary>
    /// Creates message data area information.
    /// </summary>
    /// <param name="position">position on game plan</param>
    /// <param name="area">area of the game board</param>
    /// <param name="img">image that will be shown</param>
    public AreaInfo(Coordinates position, Area area, int img)
    {
      Position = position;
      Area = area;
      IMG = img;
    }
  }
}