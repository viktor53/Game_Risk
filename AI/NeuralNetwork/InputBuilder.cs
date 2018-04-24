using Risk.Model.Enums;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.NeuralNetwork
{
  internal static class InputBuilder
  {
    private static void AddSurroundingsInfoToInput(ArmyColor aiColor, Area area, IList<Area> areas, IList<IList<bool>> connections,
      int levels, int offset, double[] input)
    {
      SurroundingsInformation info = NeuroHelper.GetSurroundingsInfo(area, areas, connections, levels, aiColor);

      for (int i = 0; i < info.NumberOfFriends.Length; ++i)
      {
        input[offset + i] = info.NumberOfFriends[i];
      }
      offset += levels;

      for (int i = 0; i < info.FriendlyArmies.Length; ++i)
      {
        input[offset + i] = info.FriendlyArmies[i];
      }
      offset += levels;

      for (int i = 0; i < info.NumberOfEnemies.Length; ++i)
      {
        input[offset + i] = info.NumberOfEnemies[i];
      }
      offset += levels;

      for (int i = 0; i < info.EnemyArmies.Length; ++i)
      {
        input[offset + i] = info.EnemyArmies[i];
      }
    }

    public static void PrepareSetUpInput(ArmyColor aiColor, Area area, IList<Area> areas, IList<IList<bool>> connections,
      IDictionary<int, RegionInformation> regionsInfo, double[] input)
    {
      const int levels = 2;

      if (area.ArmyColor == ArmyColor.Neutral)
      {
        input[0] = 0;
      }
      else
      {
        input[0] = 1;
      }

      AddSurroundingsInfoToInput(aiColor, area, areas, connections, levels, 1, input);

      int offset = levels * 4;

      input[offset] = regionsInfo[area.RegionID].NumberOfAreas;
      offset++;

      Tuple<int, int> stateOfRegion = NeuroHelper.GetRegionState(areas, area.RegionID, regionsInfo, aiColor);

      input[offset] = stateOfRegion.Item1;
      offset++;
      input[offset] = stateOfRegion.Item2;
      offset++;

      input[offset] = regionsInfo[area.RegionID].BonusForRegion;
      offset++;

      input[offset] = regionsInfo[area.RegionID].NumberOfAreasForOneArmy;
      offset++;

      input[offset] = regionsInfo[area.RegionID].NumberOfDefendArmies;
      offset++;

      input[offset] = regionsInfo[area.RegionID].DefendRate;
    }

    public static void PrepareDraftInput(ArmyColor aiColor, Area area, IList<Area> areas,
      IList<IList<bool>> connections, double[] input)
    {
      const int levels = 2;

      input[0] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(aiColor, area, areas, connections, levels, 1, input);
    }

    public static void PrepareExchangeCardInput(ArmyColor aiColor, int freeUnit, int numberOfCombination,
      IList<Area> areas, IList<IList<bool>> connections, double[] input)
    {
      input[0] = freeUnit;
      input[1] = numberOfCombination;

      Tuple<int, int> state = NeuroHelper.GetStateOfBorders(areas, connections, aiColor);

      input[2] = state.Item1;
      input[3] = state.Item2;
    }

    public static void PrepareAttackFromInput(ArmyColor aiColor, Area area, IList<Area> areas,
      IList<IList<bool>> connections, double[] input)
    {
      const int levels = 2;

      input[0] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(aiColor, area, areas, connections, levels, 2, input);
    }

    public static void PrepareAttackToInput(ArmyColor aiColor, Area area, IList<Area> areas,
      IDictionary<int, RegionInformation> regionsInfo, double[] input)
    {
      const int levels = 2;

      input[1] = area.SizeOfArmy;

      int offset = 2 + levels * 4;

      input[offset] = regionsInfo[area.RegionID].NumberOfAreas;
      offset++;

      Tuple<int, int> stateOfRegion = NeuroHelper.GetRegionState(areas, area.RegionID, regionsInfo, aiColor);

      input[offset] = stateOfRegion.Item1;
      offset++;
      input[offset] = stateOfRegion.Item2;
      offset++;

      input[offset] = regionsInfo[area.RegionID].BonusForRegion;
      offset++;

      input[offset] = regionsInfo[area.RegionID].NumberOfAreasForOneArmy;
      offset++;

      input[offset] = regionsInfo[area.RegionID].NumberOfDefendArmies;
      offset++;

      input[offset] = regionsInfo[area.RegionID].DefendRate;
    }

    public static void PrepareFortifyFromInput(ArmyColor aiColor, Area area, IList<Area> areas,
      IList<IList<bool>> connections, double[] input)
    {
      const int levels = 2;

      input[0] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(aiColor, area, areas, connections, levels, 2, input);
    }

    public static void PrepareFortifyToInput(ArmyColor aiColor, Area area, IList<Area> areas,
      IList<IList<bool>> connections, double[] input)
    {
      const int levels = 2;

      input[1] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(aiColor, area, areas, connections, levels, 10, input);
    }

    public static int GetAttackSize(double result, int sizeOfArmy)
    {
      const double oneArmy = 0.33;
      const double twoArmies = 0.66;

      if (twoArmies <= result && 3 < sizeOfArmy)
      {
        return 3;
      }

      if (oneArmy <= result && result < twoArmies && 2 < sizeOfArmy)
      {
        return 2;
      }

      if (result < oneArmy)
      {
        return 1;
      }

      return sizeOfArmy - 1;
    }
  }
}