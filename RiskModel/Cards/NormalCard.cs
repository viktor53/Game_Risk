using Risk.Model.Enums;

namespace Risk.Model.Cards
{
  /// <summary>
  /// Represents normal risk card with unit type and area.
  /// </summary>
  public sealed class NormalCard : RiskCard
  {
    /// <summary>
    /// ID of area on the risk card.
    /// </summary>
    public int Area { get; private set; }

    /// <summary>
    /// Creates normal risk card.
    /// </summary>
    /// <param name="type">unit type on the card</param>
    /// <param name="area">ID of area on the card</param>
    public NormalCard(UnitType type, int area)
    {
      TypeUnit = type;
      Area = area;
    }
  }
}