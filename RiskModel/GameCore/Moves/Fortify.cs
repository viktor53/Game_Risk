using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move fortify.
  /// </summary>
  public sealed class Fortify : Move
  {
    /// <summary>
    /// Area of player, where units will be taken.
    /// </summary>
    public int FromAreaID { get; private set; }

    /// <summary>
    /// Area of player, where units will be moved.
    /// </summary>
    public int ToAreaID { get; private set; }

    /// <summary>
    /// Number of unit to moving.
    /// </summary>
    public int SizeOfArmy { get; private set; }

    /// <summary>
    /// Creates game move fortify.
    /// </summary>
    /// <param name="armyColor">color of player, who makes fortify</param>
    /// <param name="fromAreaID">area of player, where units will be taken</param>
    /// <param name="toAreaID">area of player, where units will be moved</param>
    /// <param name="sizeOfArmy">number of unit to moving</param>
    public Fortify(ArmyColor armyColor, int fromAreaID, int toAreaID, int sizeOfArmy) : base(armyColor)
    {
      FromAreaID = fromAreaID;
      ToAreaID = toAreaID;
      SizeOfArmy = sizeOfArmy;
    }
  }
}