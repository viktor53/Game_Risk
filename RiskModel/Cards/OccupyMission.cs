using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Represents mission card with target to occupy the number of region.
  /// </summary>
  public sealed class OccupyMission : MissionCard
  {
    /// <summary>
    /// Number of regions that need to be occupied.
    /// </summary>
    public int CountOfRegion { get; private set; }

    /// <summary>
    /// Creates occupy mission card.
    /// </summary>
    /// <param name="countOfRegion">number of regions to be occupied</param>
    public OccupyMission(int countOfRegion)
    {
      Target = MissionTarget.Occupy;
      CountOfRegion = countOfRegion;
    }
  }
}