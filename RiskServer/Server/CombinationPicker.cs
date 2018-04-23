using Risk.Model.Cards;
using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Server
{
  internal static class CombinationPicker
  {
    /// <summary>
    /// Gets risk cards combination if it exists.
    /// </summary>
    /// <param name="cardsInHand">list of cards in hand</param>
    /// <returns>risk cards combination or empty combination</returns>
    public static IList<RiskCard> GetCombination(IList<RiskCard> cardsInHand)
    {
      IList<RiskCard> combination;

      combination = GetMixCombination(cardsInHand);

      if (combination.Count == 3) return combination;

      combination = GetSameCombination(cardsInHand);

      return combination;
    }

    /// <summary>
    /// Gets mix risk cards combination.
    /// </summary>
    /// <param name="cardsInHand">list of cards in hand</param>
    /// <returns>mix risk cards combination or not full combination</returns>
    private static IList<RiskCard> GetMixCombination(IList<RiskCard> cardsInHand)
    {
      IList<RiskCard> combination = new List<RiskCard>();

      bool isInfatry = false;
      bool isCavalery = false;
      bool isCannon = false;
      bool isMix = false;
      foreach (var card in cardsInHand)
      {
        if (combination.Count < 3)
        {
          switch (card.TypeUnit)
          {
            case UnitType.Infantry:
              if (!isInfatry)
              {
                combination.Add(card);
                isInfatry = true;
              }
              break;

            case UnitType.Cavalary:
              if (!isCavalery)
              {
                combination.Add(card);
                isCavalery = true;
              }
              break;

            case UnitType.Cannon:
              if (!isCannon)
              {
                combination.Add(card);
                isCavalery = true;
              }
              break;

            case UnitType.Mix:
              if (!isMix)
              {
                combination.Add(card);
                isMix = true;
              }
              break;
          }
        }
      }

      return combination;
    }

    /// <summary>
    /// Gets same risk cards combination.
    /// </summary>
    /// <param name="cardsInHand">list of cards in hand</param>
    /// <returns>same risk cards combination or empty combination</returns>
    private static IList<RiskCard> GetSameCombination(IList<RiskCard> cardsInHand)
    {
      for (int i = 0; i < 4; ++i)
      {
        IList<RiskCard> combination = new List<RiskCard>();

        bool isMix = false;
        foreach (var card in cardsInHand)
        {
          if (card.TypeUnit == (UnitType)i)
          {
            combination.Add(card);
          }
          else if (card.TypeUnit == UnitType.Mix && !isMix)
          {
            combination.Add(card);
            isMix = true;
          }
        }

        if (combination.Count == 3) return combination;
      }

      return new List<RiskCard>();
    }
  }
}