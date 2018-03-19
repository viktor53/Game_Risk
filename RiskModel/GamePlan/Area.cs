using Risk.Model.Enums;

namespace Risk.Model.GamePlan
{
  /// <summary>
  /// Represents an area on game board.
  /// </summary>
  public sealed class Area
  {
    /// <summary>
    /// ID of area.
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// ID of region where area is.
    /// </summary>
    public int RegionID { get; private set; }

    /// <summary>
    /// Color of player who occupied the area.
    /// </summary>
    public ArmyColor ArmyColor { get; set; }

    /// <summary>
    /// Number of units in the area.
    /// </summary>
    public int SizeOfArmy { get; set; }

    /// <summary>
    /// Creates neutral area with no army.
    /// </summary>
    /// <param name="id">ID of area</param>
    /// <param name="regionID">ID of region</param>
    public Area(int id, int regionID)
    {
      ID = id;
      RegionID = regionID;
      ArmyColor = ArmyColor.Neutral;
      SizeOfArmy = 0;
    }
  }
}