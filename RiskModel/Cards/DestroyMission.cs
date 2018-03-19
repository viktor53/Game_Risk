using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Represents mission card with target to destroy the player.
  /// </summary>
  public sealed class DestroyMission : MissionCard
  {
    /// <summary>
    /// Color of player that need to be destroied.
    /// </summary>
    public ArmyColor ArmyColor { get; private set; }

    /// <summary>
    /// Creates destroy mission card.
    /// </summary>
    /// <param name="armyColor">color of player to be destroied</param>
    public DestroyMission(ArmyColor armyColor)
    {
      Target = MissionTarget.Destroy;
      ArmyColor = armyColor;
    }
  }
}