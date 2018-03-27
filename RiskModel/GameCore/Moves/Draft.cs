using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move draft.
  /// </summary>
  public struct Draft
  {
    /// <summary>
    /// Color of player, who makes move.
    /// </summary>
    public ArmyColor PlayerColor { get; private set; }

    /// <summary>
    /// Area of player, where units will be placed.
    /// </summary>
    public int AreaID { get; private set; }

    /// <summary>
    /// Number of unit that will be placed.
    /// </summary>
    public int NumberOfUnit { get; private set; }

    /// <summary>
    /// Creates game move draft.
    /// </summary>
    /// <param name="armyColor">color of player, who makes draft</param>
    /// <param name="areaID">area of player</param>
    /// <param name="numberOfUnit">number of unit</param>
    public Draft(ArmyColor armyColor, int areaID, int numberOfUnit)
    {
      PlayerColor = armyColor;
      AreaID = areaID;
      NumberOfUnit = numberOfUnit;
    }
  }
}