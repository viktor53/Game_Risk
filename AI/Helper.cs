using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.GamePlan;
using Risk.Model.Enums;
using Risk.Model.Cards;

namespace Risk.AI
{
  /// <summary>
  /// Provides methods for finding possible moves.
  /// </summary>
  internal static class Helper
  {
    /// <summary>
    /// Gets unoccupied areas.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <returns>unoccupied areas</returns>
    public static IList<Area> GetUnoccupiedAreas(IEnumerable<Area> areas)
    {
      List<Area> unoccupiedAreas = new List<Area>();
      foreach (var area in areas)
      {
        if (area.ArmyColor == ArmyColor.Neutral)
        {
          unoccupiedAreas.Add(area);
        }
      }
      return unoccupiedAreas;
    }

    /// <summary>
    /// Gets own areas.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>own areas</returns>
    public static IList<Area> GetMyAreas(IEnumerable<Area> areas, ArmyColor aiColor)
    {
      List<Area> myAreas = new List<Area>();
      foreach (var area in areas)
      {
        if (area.ArmyColor == aiColor)
        {
          myAreas.Add(area);
        }
      }
      return myAreas;
    }

    /// <summary>
    /// Gets all areas that can attack.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>areas that can attack</returns>
    public static IList<Area> WhoCanAttack(IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> canAttack = new List<Area>();
      foreach (var area in areas)
      {
        if (area.ArmyColor == aiColor && CanAttack(area, areas, connections, aiColor))
        {
          canAttack.Add(area);
        }
      }
      return canAttack;
    }

    /// <summary>
    /// Finds out if the area can attack.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>true if the area can attack, otherwise false</returns>
    public static bool CanAttack(Area area, IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      if (area.SizeOfArmy > 1)
      {
        for (int i = 0; i < connections[area.ID].Count; ++i)
        {
          if (connections[area.ID][i] && areas[i].ArmyColor != aiColor)
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Gets all areas where the area can attack using heuristic.
    /// </summary>
    /// <param name="area">the attacking area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>areas where the area can attack</returns>
    public static IList<Area> WhoCanBeAttacked(Area area, IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> canBeAttacked = new List<Area>();
      for (int i = 0; i < connections[area.ID].Count; ++i)
      {
        if (connections[area.ID][i] && areas[i].ArmyColor != aiColor)
        {
          canBeAttacked.Add(areas[i]);
        }
      }
      return canBeAttacked;
    }

    /// <summary>
    /// Gets maximum size of attack.
    /// </summary>
    /// <param name="area">the attacking area</param>
    /// <returns>maximum size of attack</returns>
    public static int GetMaxSizeOfAttack(Area area)
    {
      if (area.SizeOfArmy <= 1)
      {
        return 0;
      }

      if (area.SizeOfArmy <= 4)
      {
        return area.SizeOfArmy - 1;
      }

      return 3;
    }

    /// <summary>
    /// Gets all areas that can make fortify move.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>areas that can make fortify move</returns>
    public static IList<Area> WhoCanFortify(IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> canFortify = new List<Area>();
      foreach (var area in areas)
      {
        if (area.ArmyColor == aiColor && area.SizeOfArmy > 1 && HasFriendlyNeighbors(area, areas, connections, aiColor))
        {
          canFortify.Add(area);
        }
      }
      return canFortify;
    }

    /// <summary>
    /// Finds out if the area has friendly neighbours.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>true if the area has friendly neighbours, otherwise false</returns>
    private static bool HasFriendlyNeighbors(Area area, IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      for (int i = 0; i < connections[area.ID].Count; ++i)
      {
        if (connections[area.ID][i] && areas[i].ArmyColor == aiColor)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Gets all areas where the area can make fortify move.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>areas where the area can make fortify move</returns>
    public static IList<Area> WhereCanFortify(Area area, IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> where = new List<Area>();
      HashSet<Area> visited = new HashSet<Area>();
      Queue<Area> toVisit = new Queue<Area>();
      toVisit.Enqueue(area);
      while (toVisit.Count != 0)
      {
        Area a = toVisit.Dequeue();
        visited.Add(a);
        where.Add(a);
        for (int i = 0; i < connections[a.ID].Count; ++i)
        {
          if (connections[a.ID][i] && areas[i].ArmyColor == aiColor && !visited.Contains(areas[i]))
          {
            toVisit.Enqueue(areas[i]);
          }
        }
      }

      where.Remove(area);
      return where;
    }

    /// <summary>
    /// Gets a card combination.
    /// </summary>
    /// <param name="cardsInHand">cards in a hand</param>
    /// <returns>a card combination</returns>
    public static IList<RiskCard> GetCombination(IEnumerable<RiskCard> cardsInHand)
    {
      IList<RiskCard> combination;
      combination = GetSameCombination(cardsInHand);
      if (combination.Count == 3)
      {
        return combination;
      }

      return GetMixCombination(cardsInHand);
    }

    /// <summary>
    /// Gets a card combination with mixed types.
    /// </summary>
    /// <param name="cardsInHand">cards in a hand</param>
    /// <returns>a card combination</returns>
    private static IList<RiskCard> GetMixCombination(IEnumerable<RiskCard> cardsInHand)
    {
      List<RiskCard> combination = new List<RiskCard>();
      bool isInfanrty, isCavalary, isCannon, isMix;
      isInfanrty = false;
      isCavalary = false;
      isCannon = false;
      isMix = false;

      foreach (var card in cardsInHand)
      {
        switch (card.TypeUnit)
        {
          case UnitType.Infantry:
            if (!isInfanrty)
            {
              combination.Add(card);
              isInfanrty = true;
            }
            break;

          case UnitType.Cavalary:
            if (!isCavalary)
            {
              combination.Add(card);
              isCavalary = true;
            }
            break;

          case UnitType.Cannon:
            if (!isCannon)
            {
              combination.Add(card);
              isCannon = true;
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
        if (combination.Count == 3)
          return combination;
      }

      combination.Clear();
      return combination;
    }

    /// <summary>
    /// Gets a card combination with the same type.
    /// </summary>
    /// <param name="cardsInHand">cards in a hand</param>
    /// <returns>a card combination</returns>
    private static IList<RiskCard> GetSameCombination(IEnumerable<RiskCard> cardsInHand)
    {
      IList<RiskCard> combination;

      combination = GetTypeOfCombination(cardsInHand, UnitType.Infantry);
      if (combination.Count == 3)
      {
        return combination;
      }

      combination = GetTypeOfCombination(cardsInHand, UnitType.Cavalary);
      if (combination.Count == 3)
      {
        return combination;
      }

      return GetTypeOfCombination(cardsInHand, UnitType.Cannon);
    }

    /// <summary>
    /// Gets a card combination of the type.
    /// </summary>
    /// <param name="cardsInHand">cards in a hand</param>
    /// <param name="typeOfCom">the type of combination</param>
    /// <returns>a card combination</returns>
    private static IList<RiskCard> GetTypeOfCombination(IEnumerable<RiskCard> cardsInHand, UnitType typeOfCom)
    {
      List<RiskCard> combination = new List<RiskCard>();

      bool isMix = false;
      foreach (var card in cardsInHand)
      {
        if (card.TypeUnit == typeOfCom)
        {
          combination.Add(card);
        }
        if (card.TypeUnit == UnitType.Mix && !isMix)
        {
          combination.Add(card);
          isMix = true;
        }
        if (combination.Count == 3)
        {
          return combination;
        }
      }

      combination.Clear();
      return combination;
    }
  }
}