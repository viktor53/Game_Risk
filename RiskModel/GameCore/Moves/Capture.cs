using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move capture.
  /// </summary>
  public sealed class Capture : Move
  {
    /// <summary>
    /// Number of unit to moving.
    /// </summary>
    public int ArmyToMove { get; private set; }

    /// <summary>
    /// Creates game move capture.
    /// </summary>
    /// <param name="armyColor">color of player who capture</param>
    /// <param name="armyToMove">number of unit</param>
    public Capture(ArmyColor armyColor, int armyToMove) : base(armyColor)
    {
      ArmyToMove = armyToMove;
    }
  }
}