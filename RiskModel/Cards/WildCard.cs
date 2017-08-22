using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  sealed class WildCard : RiskCard
  {
    public WildCard()
    {
      TypeUnit = UnitType.Mix;
    }
  }
}
