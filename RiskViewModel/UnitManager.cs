using System.Collections.Generic;

namespace Risk.ViewModel
{
  class UnitManager
  {
    //private GameBoard _board;
    //private int[] _armyForContinent;

    //public UnitManager(GameBoard board, int[] armyForContinent)
    //{
    //  _board = board;
    //  _armyForContinent = armyForContinent;
    //}

    //private int GetArmyFromAreas(ArmyColor armyColor)
    //{
    //  int countOfAreas = 0;
    //  for (int i = 0; i < _board.Areas.Length; i++)
    //  {
    //    if (_board.Areas[i].ArmyColor == armyColor)
    //    {
    //      countOfAreas++;
    //    }
    //  }
    //  return countOfAreas / 3;
    //}

    //private int GetArmyFromContinents(ArmyColor armyColor)
    //{
    //  int newArmies = 0;

    //  for (int continent = 0; continent < _armyForContinent.Length; continent++)
    //  {
    //    bool isCapturedContinent = true;

    //    for (int i = 0; i < _board.Areas.Length; i++)
    //    {
    //      if ((int)_board.Areas[i].Reg != continent || _board.Areas[i].ArmyColor != armyColor)
    //      {
    //        isCapturedContinent = false;
    //        break;
    //      }
    //    }

    //    if (isCapturedContinent)
    //    {
    //      newArmies += _armyForContinent[continent];
    //    }
    //  }

    //  return newArmies;
    //}

    //private void RegroupUnit(int area)
    //{
    //  _board.Areas[area];
    //}

    //public void RisePlayerArmyBeforTurn(IPlayer player)
    //{
    //  int newArmies = GetArmyFromAreas(player.GetColor()) + GetArmyFromContinents(player.GetColor());

    //  if(newArmies < 3)
    //  {
    //    newArmies = 3;
    //  }

    //  player.RiseArmy(newArmies);
    //}

    //public void RisePlayerArmyByCard(IPlayer player, IList<RiskCard> triplet)
    //{

    //}

    //public void RiseAreaArmy(int area, int newArmy)
    //{
    //  _board.Areas[area].SizeOfArmy += newArmy;

    //  RegroupUnit(area);
    //}
  }
}
