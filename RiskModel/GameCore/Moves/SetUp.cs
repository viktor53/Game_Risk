using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move setup.
  /// </summary>
  public sealed class SetUp : Move
  {
    /// <summary>
    /// Area of player or neutral area, where one unit will be placed.
    /// </summary>
    public int AreaID { get; private set; }

    /// <summary>
    /// Creates game move setup.
    /// </summary>
    /// <param name="armyColor">color of player, who makes setup</param>
    /// <param name="areaID">area of player or neutral area</param>
    public SetUp(ArmyColor armyColor, int areaID) : base(armyColor)
    {
      AreaID = areaID;
    }
  }
}