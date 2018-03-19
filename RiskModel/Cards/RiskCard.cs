using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Base class representing risk card.
  /// </summary>
  public abstract class RiskCard
  {
    /// <summary>
    /// Type of unit on the card.
    /// </summary>
    public UnitType TypeUnit { get; set; }
  }
}