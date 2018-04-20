using Risk.Model.Enums;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.NeuralNetwork
{
  internal static class NeuroHelper
  {
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