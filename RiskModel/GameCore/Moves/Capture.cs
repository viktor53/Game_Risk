using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move capture.
  /// </summary>
  public struct Capture
  {
    /// <summary>
    /// Color of player, who makes move.
    /// </summary>
    public ArmyColor PlayerColor { get; private set; }

    /// <summary>
    /// Number of unit to moving.
    /// </summary>
    public int ArmyToMove { get; private set; }

    /// <summary>
    /// Creates game move capture.
    /// </summary>
    /// <param name="armyColor">color of player who capture</param>
    /// <param name="armyToMove">number of unit</param>
    public Capture(ArmyColor armyColor, int armyToMove)
    {
      PlayerColor = armyColor;
      ArmyToMove = armyToMove;
    }
  }
}