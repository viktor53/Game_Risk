using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  sealed class NormalCard : RiskCard
  {
    public int Area { get; private set; }

    public NormalCard(UnitType type, int area)
    {
      TypeUnit = type;
      Area = area;
    }
  }
}
