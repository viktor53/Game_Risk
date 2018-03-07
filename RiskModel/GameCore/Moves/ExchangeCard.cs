using Risk.Model.Cards;
using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public class ExchangeCard : Move
  {
    public IList<RiskCard> Combination { get; private set; }

    public ExchangeCard(ArmyColor playerColor, IList<RiskCard> combination) : base(playerColor)
    {
      Combination = combination;
    }
  }
}