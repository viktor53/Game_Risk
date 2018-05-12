using Risk.Model.Enums;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI
{
  /// <summary>
  /// Provides methods for finding possible moves.
  /// </summary>
  internal class HeuristicHelper
  {
    /// <summary>
    /// Gets all draft possibillities using heuristic.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>all draft posibillities</returns>
    public static IList<Area> GetDraftPossibilities(IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> possibilities = new List<Area>();
      foreach (var area in areas)
      {
        if (area.ArmyColor == aiColor && IsOnBoarder(area, areas, connections))
        {
          possibilities.Add(area);
        }
      }

      return possibilities;
    }

    /// <summary>
    /// Finds out if the area is on a border. If the area is connected with at least one enemy.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <returns>true if the area is on a border, otherwise false</returns>
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

    /// <summary>
    /// Gets all areas that can attack using heuristic.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>areas that can attack</returns>
    public static IList<Area> WhoCanAttack(IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> posibilities = new List<Area>();

      foreach (var area in areas)
      {
        if (area.ArmyColor == aiColor && area.SizeOfArmy > 1)
        {
          for (int i = 0; i < connections[area.ID].Count; ++i)
          {
            if (connections[area.ID][i] && areas[i].ArmyColor != aiColor && IsGoodAttack(area, areas[i]))
            {
              posibilities.Add(area);
              break;
            }
          }
        }
      }

      return posibilities;
    }

    /// <summary>
    /// Gets all areas where the area can attack using heuristic.
    /// </summary>
    /// <param name="area">the attacking area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <returns>areas where the area can attack</returns>
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

    /// <summary>
    /// Finds out if it is good attack.
    /// </summary>
    /// <param name="attacker">attacking area</param>
    /// <param name="defender">defending area</param>
    /// <returns>true if it is good attack, otherwise false</returns>
    public static bool IsGoodAttack(Area attacker, Area defender)
    {
      return attacker.SizeOfArmy - 1 > defender.SizeOfArmy;
    }

    /// <summary>
    /// Gets all areas that can make fortify move using heuristic.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>areas that can make fortify move</returns>
    public static IList<Area> WhoCanFortify(IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      List<Area> posibilities = new List<Area>();

      foreach (var area in areas)
      {
        if (area.ArmyColor == aiColor && area.SizeOfArmy > 1 && HasOnlyFriendlyNeighbours(area, areas, connections))
        {
          posibilities.Add(area);
        }
      }

      return posibilities;
    }

    /// <summary>
    /// Finds out if the area has only friendly neighbours.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <returns>true if the area has only friendly neighbours, otherwise false</returns>
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

    /// <summary>
    /// Gets all areas where the area can make fortify move using heuristic.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <returns>areas where the area can make fortify move</returns>
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