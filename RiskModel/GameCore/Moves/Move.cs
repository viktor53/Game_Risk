using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Base class representing game move.
  /// </summary>
  public abstract class Move
  {
    /// <summary>
    /// Color of player, who makes move.
    /// </summary>
    public ArmyColor PlayerColor { get; private set; }

    /// <summary>
    /// Base constructor.
    /// </summary>
    /// <param name="playerColor">color of player, who makes move</param>
    public Move(ArmyColor playerColor)
    {
      PlayerColor = playerColor;
    }
  }
}