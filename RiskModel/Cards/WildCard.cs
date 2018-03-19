using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Represents wild card/joker with mix unit type.
  /// </summary>
  public sealed class WildCard : RiskCard
  {
    /// <summary>
    /// Creates risk wild card.
    /// </summary>
    public WildCard()
    {
      TypeUnit = UnitType.Mix;
    }
  }
}