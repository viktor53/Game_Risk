using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Base class for mission cards.
  /// </summary>
  public abstract class MissionCard
  {
    /// <summary>
    /// Target of mission that need to be reached.
    /// </summary>
    public MissionTarget Target { get; protected set; }
  }
}