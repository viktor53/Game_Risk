using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  sealed class DestroyMission : MissionCard
  {
    public ArmyColor ArmyColor { get; private set; }

    public DestroyMission(ArmyColor armyColor)
    {
      Target = MissionTarget.Destroy;
      ArmyColor = armyColor;
    }
  }
}
