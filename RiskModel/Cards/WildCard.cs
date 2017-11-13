using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  public sealed class WildCard : RiskCard
  {
    public WildCard()
    {
      TypeUnit = UnitType.Mix;
    }
  }
}
