using Risk.Model.Enums;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI
{
  internal class HeuristicHelper
  {
    public static IList<Area> GetDraftPosibilities(IList<Area> areas, IList<IList<bool>> connections, ArmyColor playerColor)
    {
      List<Area> posibilities = new List<Area>();
      foreach (var area in areas)
      {
        if (area.ArmyColor == playerColor && IsOnBoarder(area, areas, connections))
        {
          posibilities.Add(area);
        }
      }

      return posibilities;
    }

    private static bool IsOnBoarder(Area area, IList<Area> areas, IList<IList<bool>> connections)
    {
      for (int i = 0; i < connections[area.ID].Count; ++i)
      {
        if (connections[area.ID][i] && areas[i].ArmyColor != area.ArmyColor)
        {
          return true;
        }
      }
      return false;
    }

    public static IList<Area> WhoCanAttack(IList<Area> areas, IList<IList<bool>> connections, ArmyColor playerColor)
    {
      List<Area> posibilities = new List<Area>();

      foreach (var area in areas)
      {
        if (area.ArmyColor == playerColor && area.SizeOfArmy > 1)
        {
          for (int i = 0; i < connections[area.ID].Count; ++i)
          {
            if (connections[area.ID][i] && areas[i].ArmyColor != playerColor && IsGoodAttack(area, areas[i]))
            {
              posibilities.Add(area);
              break;
            }
          }
        }
      }

      return posibilities;
    }

    public static IList<Area> WhereCanAttack(Area area, IList<Area> areas, IList<IList<bool>> connections)
    {
      List<Area> posibilities = new List<Area>();

      for (int i = 0; i < connections[area.ID].Count; ++i)
      {
        if (connections[area.ID][i] && areas[i].ArmyColor != area.ArmyColor && IsGoodAttack(area, areas[i]))
        {
          posibilities.Add(areas[i]);
        }
      }

      return posibilities;
    }

    public static bool IsGoodAttack(Area attacker, Area defender)
    {
      return attacker.SizeOfArmy - 1 > defender.SizeOfArmy;
    }

    public static IList<Area> WhoCanFortify(IList<Area> areas, IList<IList<bool>> connections, ArmyColor playerColor)
    {
      List<Area> posibilities = new List<Area>();

      foreach (var area in areas)
      {
        if (area.ArmyColor == playerColor && area.SizeOfArmy > 1 && HasOnlyFriendlyNeighbours(area, areas, connections))
        {
          posibilities.Add(area);
        }
      }

      return posibilities;
    }

    private static bool HasOnlyFriendlyNeighbours(Area area, IList<Area> areas, IList<IList<bool>> connections)
    {
      for (int i = 0; i < connections[area.ID].Count; ++i)
      {
        if (connections[area.ID][i] && areas[i].ArmyColor != area.ArmyColor)
        {
          return false;
        }
      }
      return true;
    }

    public static IList<Area> WhereCanFortify(Area area, IList<Area> areas, IList<IList<bool>> connections)
    {
      List<Area> posibilities = new List<Area>();

      HashSet<Area> visited = new HashSet<Area>();
      Queue<Area> toVisit = new Queue<Area>();
      toVisit.Enqueue(area);

      while (toVisit.Count != 0)
      {
        Area a = toVisit.Dequeue();
        visited.Add(a);
        if (IsOnBoarder(a, areas, connections))
        {
          posibilities.Add(a);
        }
        for (int i = 0; i < connections[a.ID].Count; ++i)
        {
          if (connections[a.ID][i] && areas[i].ArmyColor == area.ArmyColor && !visited.Contains(areas[i]))
          {
            toVisit.Enqueue(areas[i]);
          }
        }
      }

      return posibilities;
    }
  }
}