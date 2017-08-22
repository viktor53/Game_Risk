using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  sealed class ConquerMission : MissionCard
  {
    public Region[] Targets { get; private set; }

    public ConquerMission(Region[] targets)
    {
      Target = MissionTarget.Conquer;
      Targets = targets;
    }
  }
}
