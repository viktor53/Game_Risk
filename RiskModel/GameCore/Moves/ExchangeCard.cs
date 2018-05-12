using System.Collections.Generic;
using Risk.Model.Cards;
using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move exchange card.
  /// </summary>
  public sealed class ExchangeCard : Move
  {
    /// <summary>
    /// Combination of three risk cards.
    /// </summary>
    public IList<RiskCard> Combination { get; private set; }

    /// <summary>
    /// Creates game move exchange card.
    /// </summary>
    /// <param name="playerColor">color of player, who makes exchange</param>
    /// <param name="combination">combination of three risk cards</param>
    public ExchangeCard(ArmyColor playerColor, IList<RiskCard> combination) : base(playerColor)
    {
      Combination = combination;
    }
  }
}