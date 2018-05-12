using Risk.Model.Enums;
using System;

namespace Risk.Model.GamePlan
{
  /// <summary>
  /// Represents an area on game board.
  /// </summary>
  public sealed class Area : ICloneable
  {
    /// <summary>
    /// ID of area.
    /// </summary>
    public byte ID { get; private set; }

    /// <summary>
    /// ID of region where area is.
    /// </summary>
    public byte RegionID { get; private set; }

    /// <summary>
    /// Color of player who occupied the area.
    /// </summary>
    public ArmyColor ArmyColor { get; set; }

    /// <summary>
    /// Number of units in the area.
    /// </summary>
    public int SizeOfArmy { get; set; }

    /// <summary>
    /// Copy contructor.
    /// </summary>
    /// <param name="area">original area</param>
    private Area(Area area)
    {
      ID = area.ID;
      RegionID = area.RegionID;
      ArmyColor = area.ArmyColor;
      SizeOfArmy = area.SizeOfArmy;
    }

    /// <summary>
    /// Creates neutral area with no army.
    /// </summary>
    /// <param name="id">ID of area</param>
    /// <param name="regionID">ID of region</param>
    public Area(byte id, byte regionID)
    {
      ID = id;
      RegionID = regionID;
      ArmyColor = ArmyColor.Neutral;
      SizeOfArmy = 0;
    }

    public object Clone()
    {
      return new Area(this);
    }
  }
}