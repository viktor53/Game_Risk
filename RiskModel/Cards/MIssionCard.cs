using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  abstract class MissionCard
  {
    public MissionTarget Target { get; protected set; }
  }
}
