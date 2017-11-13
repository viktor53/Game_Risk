using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  public sealed class OccupyMission : MissionCard
  {
    public int CountOfRegion { get; private set; }

    public OccupyMission(int countOfRegion)
    {
      Target = MissionTarget.Occupy;
      CountOfRegion = countOfRegion;
    }
  }
}
