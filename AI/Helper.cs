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
  internal static class Helper
  {
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

    public static IList<Area> WhoCanAttack(GamePlanInfo gamePlan, ArmyColor aiColor)
    {
      List<Area> canAttack = new List<Area>();
      foreach (var area in gamePlan.Areas)
      {
        if (area.ArmyColor == aiColor && CanAttack(area, gamePlan, aiColor))
        {
          canAttack.Add(area);
        }
      }
      return canAttack;
    }

    public static bool CanAttack(Area area, GamePlanInfo gamePlan, ArmyColor aiColor)
    {
      if (area.SizeOfArmy > 1)
      {
        for (int i = 0; i < gamePlan.Connections[area.ID].Length; ++i)
        {
          if (gamePlan.Connections[area.ID][i] && gamePlan.Areas[i].ArmyColor != aiColor)
          {
            return true;
          }
        }
      }
      return false;
    }

    public static IList<Area> WhoCanBeAttacked(Area area, GamePlanInfo gamePlan, ArmyColor aiColor)
    {
      List<Area> canBeAttacked = new List<Area>();
      for (int i = 0; i < gamePlan.Connections[area.ID].Length; ++i)
      {
        if (gamePlan.Connections[area.ID][i] && gamePlan.Areas[i].ArmyColor != aiColor)
        {
          canBeAttacked.Add(gamePlan.Areas[i]);
        }
      }
      return canBeAttacked;
    }

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

    public static IList<Area> WhoCanFortify(GamePlanInfo gamePlan, ArmyColor aiColor)
    {
      List<Area> canFortify = new List<Area>();
      foreach (var area in gamePlan.Areas)
      {
        if (area.ArmyColor == aiColor && area.SizeOfArmy > 1 && HasFriendlyNeighbors(area, gamePlan, aiColor))
        {
          canFortify.Add(area);
        }
      }
      return canFortify;
    }

    private static bool HasFriendlyNeighbors(Area area, GamePlanInfo gamePlan, ArmyColor aiColor)
    {
      for (int i = 0; i < gamePlan.Connections[area.ID].Length; ++i)
      {
        if (gamePlan.Connections[area.ID][i] && gamePlan.Areas[i].ArmyColor == aiColor)
        {
          return true;
        }
      }
      return false;
    }

    public static IList<Area> WhereCanFortify(Area area, GamePlanInfo gamePlan, ArmyColor aiColor)
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
        for (int i = 0; i < gamePlan.Connections[a.ID].Length; ++i)
        {
          if (gamePlan.Connections[a.ID][i] && gamePlan.Areas[i].ArmyColor == aiColor && !visited.Contains(gamePlan.Areas[i]))
          {
            toVisit.Enqueue(gamePlan.Areas[i]);
          }
        }
      }

      where.Remove(area);
      return where;
    }

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

    private static IList<RiskCard> GetSameCombination(IEnumerable<RiskCard> cardsInHand)
    {
      IList<RiskCard> combination;

      combination = GetOfTypeCombination(cardsInHand, UnitType.Infantry);
      if (combination.Count == 3)
      {
        return combination;
      }

      combination = GetOfTypeCombination(cardsInHand, UnitType.Cavalary);
      if (combination.Count == 3)
      {
        return combination;
      }

      return GetOfTypeCombination(cardsInHand, UnitType.Cannon);
    }

    private static IList<RiskCard> GetOfTypeCombination(IEnumerable<RiskCard> cardsInHand, UnitType typeOfCom)
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