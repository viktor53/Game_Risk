using Risk.Model.Enums;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.NeuralNetwork
{
  /// <summary>
  /// Provides methods for getting data for neural networks.
  /// </summary>
  internal static class NeuroHelper
  {
    /// <summary>
    /// Gets information about regions.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="bonusForRegion">bonus for region</param>
    /// <returns>inforamtion about regions</returns>
    public static IDictionary<int, RegionInformation> GetRegionsInformation(IList<Area> areas, IList<IList<bool>> connections, IList<int> bonusForRegion)
    {
      int numberOfAreas = 0;
      int numberOfBorderAreas = 0;

      var regionsInfo = new Dictionary<int, RegionInformation>();

      int currentRegion = areas[0].RegionID;
      var attackAreas = new HashSet<Area>();

      double bonus;
      double areasForArmy;
      double defendArmies;
      double defendRate;

      for (int i = 0; i < areas.Count; ++i)
      {
        if (areas[i].RegionID == currentRegion)
        {
          numberOfAreas++;

          bool borderArea = false;

          for (int j = 0; j < connections[areas[i].ID].Count; ++j)
          {
            if (connections[areas[i].ID][j] && areas[j].RegionID != currentRegion)
            {
              borderArea = true;

              if (!attackAreas.Contains(areas[j]))
              {
                attackAreas.Add(areas[j]);
              }
            }
          }

          if (borderArea)
          {
            numberOfBorderAreas++;
          }
        }
        else
        {
          bonus = 1.0 / 3.0 * numberOfAreas + bonusForRegion[currentRegion];
          areasForArmy = numberOfAreas / bonus;
          defendArmies = bonus / numberOfBorderAreas;
          defendRate = attackAreas.Count / (double)numberOfBorderAreas;

          regionsInfo.Add(currentRegion, new RegionInformation(bonus, numberOfAreas, areasForArmy, defendArmies, defendRate));

          attackAreas.Clear();
          numberOfAreas = 0;
          numberOfBorderAreas = 0;
          currentRegion = areas[i].RegionID;
          i--;
        }
      }

      bonus = 1.0 / 3.0 * numberOfAreas + bonusForRegion[currentRegion];
      areasForArmy = numberOfAreas / bonus;
      defendArmies = bonus / numberOfBorderAreas;
      defendRate = attackAreas.Count / (double)numberOfBorderAreas;

      regionsInfo.Add(currentRegion, new RegionInformation(bonus, numberOfAreas, areasForArmy, defendArmies, defendRate));

      return regionsInfo;
    }

    /// <summary>
    /// Gets a state of region. Number of owned areas and number of enemy areas.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="regionID">region ID</param>
    /// <param name="regionsInfo">information about regions</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>state of region</returns>
    public static Tuple<int, int> GetRegionState(IList<Area> areas, int regionID, IDictionary<int, RegionInformation> regionsInfo, ArmyColor aiColor)
    {
      int friends = 0;
      int enemies = 0;

      int offset = 0;
      for (int i = 0; i < regionID; ++i)
      {
        offset += regionsInfo[i].NumberOfAreas;
      }

      for (int i = 0; i < regionsInfo[regionID].NumberOfAreas; ++i)
      {
        if (areas[offset + i].ArmyColor == aiColor)
        {
          friends++;
        }
        else
        {
          enemies++;
        }
      }

      return new Tuple<int, int>(friends, enemies);
    }

    /// <summary>
    /// Gets information about surroundings of the area.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="levels">levels of surroundings</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>information about surroundings of the area</returns>
    public static SurroundingsInformation GetSurroundingsInfo(Area area, IList<Area> areas, IList<IList<bool>> connecitons, int levels, ArmyColor aiColor)
    {
      var visited = new HashSet<Area>();
      var toVisit = new Queue<Area>();

      int[] friendlyArmies = new int[levels];
      int[] enemyArmies = new int[levels];

      int[] numberOfFriends = new int[levels];
      int[] numberOfEnemies = new int[levels];

      visited.Add(area);
      AddNeighbors(area, toVisit, visited, areas, connecitons);

      for (int i = 0; i < levels; ++i)
      {
        int areasInLevel = toVisit.Count;
        for (int j = 0; j < areasInLevel; ++j)
        {
          Area a = toVisit.Dequeue();
          if (a.ArmyColor == aiColor)
          {
            numberOfFriends[i]++;
            friendlyArmies[i] += a.SizeOfArmy;
          }
          else
          {
            numberOfEnemies[i]++;
            enemyArmies[i] += a.SizeOfArmy;
          }

          AddNeighbors(a, toVisit, visited, areas, connecitons);
        }
      }

      return new SurroundingsInformation(numberOfFriends, numberOfEnemies, friendlyArmies, enemyArmies);
    }

    /// <summary>
    /// Adds neighbors of the area to toVisit and visited.
    /// </summary>
    /// <param name="area">the area</param>
    /// <param name="toVisit">areas that needs to be visit</param>
    /// <param name="visited">visited areas</param>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    private static void AddNeighbors(Area area, Queue<Area> toVisit, HashSet<Area> visited, IList<Area> areas, IList<IList<bool>> connecitons)
    {
      for (int i = 0; i < connecitons[area.ID].Count; ++i)
      {
        if (connecitons[area.ID][i] && !visited.Contains(areas[i]))
        {
          toVisit.Enqueue(areas[i]);
          visited.Add(areas[i]);
        }
      }
    }

    /// <summary>
    /// Gets a state of borders. Number of friendly armies and number of enemy armies.
    /// </summary>
    /// <param name="areas">areas on game plan</param>
    /// <param name="connections">connections of areas</param>
    /// <param name="aiColor">color of AI</param>
    /// <returns>state of borders</returns>
    public static Tuple<int, int> GetStateOfBorders(IList<Area> areas, IList<IList<bool>> connections, ArmyColor aiColor)
    {
      int friendlyArmies = 0;
      int enemyArmies = 0;

      var visited = new HashSet<Area>();

      for (int i = 0; i < areas.Count; ++i)
      {
        if (areas[i].ArmyColor == aiColor)
        {
          bool onBorder = false;

          for (int j = 0; j < connections[i].Count; ++j)
          {
            if (connections[i][j] && areas[j].ArmyColor != aiColor)
            {
              onBorder = true;

              if (!visited.Contains(areas[j]))
              {
                enemyArmies += areas[j].SizeOfArmy;
                visited.Add(areas[j]);
              }
            }
          }

          if (onBorder)
          {
            friendlyArmies += areas[i].SizeOfArmy;
          }
        }
      }

      return new Tuple<int, int>(friendlyArmies, enemyArmies);
    }
  }
}