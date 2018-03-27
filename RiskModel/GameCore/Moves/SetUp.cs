using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move setup.
  /// </summary>
  public struct SetUp
  {
    /// <summary>
    /// Color of player, who makes move.
    /// </summary>
    public ArmyColor PlayerColor { get; private set; }

    /// <summary>
    /// Area of player or neutral area, where one unit will be placed.
    /// </summary>
    public int AreaID { get; private set; }

    /// <summary>
    /// Creates game move setup.
    /// </summary>
    /// <param name="armyColor">color of player, who makes setup</param>
    /// <param name="areaID">area of player or neutral area</param>
    public SetUp(ArmyColor armyColor, int areaID)
    {
      PlayerColor = armyColor;
      AreaID = areaID;
    }
  }
}