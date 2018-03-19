using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Represents mission card with target conquer regions.
  /// </summary>
  public sealed class ConquerMission : MissionCard
  {
    /// <summary>
    /// Regions that need to be conquered.
    /// </summary>
    public Region[] Targets { get; private set; }

    /// <summary>
    /// Creates conquer mission card.
    /// </summary>
    /// <param name="targets">reigons to be conquerd</param>
    public ConquerMission(Region[] targets)
    {
      Target = MissionTarget.Conquer;
      Targets = targets;
    }
  }
}