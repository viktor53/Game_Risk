using System;
using System.Collections.Generic;
using System.Linq;
//using Risk.Model.Enums;
//using Risk.Model.Units;
//using Risk.Model.GamePlan;

namespace Risk.ViewModel
{
  public class MoveController
  {
    //private GameBoard _board;

    //private Dice _dice;

    //public MoveController(GameBoard board, Dice dice)
    //{
    //  _board = board;
    //  _dice = dice;
    //}

    //private bool IsFriendlyArea(int i, int j)
    //{
    //  return _board.Areas[i].ArmyColor == _board.Areas[j].ArmyColor;
    //}

    //private bool IsValidMove(int i, int j)
    //{
    //  return _board.Board[i][j];
    //}

    //private bool IsNeutralArea(int i)
    //{
    //  return _board.Areas[i].ArmyColor == ArmyColor.Neutral;
    //}

    //private int[] GetTwoMax(int[] rolls)
    //{
    //  int max1 = int.MinValue;
    //  int max2 = int.MinValue;

    //  for (int i = 0; i < rolls.Length; i++)
    //  {
    //    if (rolls[i] >= max1)
    //    {
    //      max2 = max1;
    //      max1 = rolls[i];
    //    }
    //    else
    //    {
    //      if(rolls[i] >= max2)
    //      {
    //        max2 = rolls[i];
    //      }
    //    }
    //  }

    //  return new int[] { max1, max2 };
    //}

    //private int CompareRolls(int[] attackerRolls, int[] deffenderRolss)
    //{
    //  switch(deffenderRolss.Length)
    //  {
    //    case 1:
    //      if (attackerRolls.Max() > deffenderRolss.Max())
    //      {
    //        return 1;
    //      }
    //      else
    //      {
    //        return -1;
    //      }
    //    case 2:
    //      var maximaAttacker = GetTwoMax(attackerRolls);
    //      var maximaDeffender = GetTwoMax(deffenderRolss);

    //      int losts = 0;

    //      for(int i = 0; i < maximaDeffender.Length; i++)
    //      {
    //        if(maximaAttacker[i] > maximaDeffender[i])
    //        {
    //          losts++;
    //        }
    //        else
    //        {
    //          losts--;
    //        }
    //      }

    //      return losts;
    //    default:
    //      throw new ArgumentException("Invalid rolls for deffender. There can be only one or two dice.");
    //  }
    //}

    //private int[] GetRolls(int sizeOfArmy, bool isAttacker)
    //{
    //  if(isAttacker)
    //  {
    //    if (sizeOfArmy > 3)
    //    {
    //      return  _dice.RollDice(3);
    //    }
    //    else
    //    {
    //      return _dice.RollDice(sizeOfArmy);
    //    }
    //  }
    //  else
    //  {
    //    if (sizeOfArmy > 2)
    //    {
    //      return _dice.RollDice(2);
    //    }
    //    else
    //    {
    //      return _dice.RollDice(sizeOfArmy);
    //    }
    //  }
    //}

    //public bool MoveUnit(int i, int j, IList<Unit> units)
    //{
    //  if(IsValidMove(i, j) && IsFriendlyArea(i, j))
    //  {
    //    foreach(Unit unit in units)
    //    {
    //      _board.Areas[j].Units.Add(unit);
    //      _board.Areas[i].Units.Remove(unit);
    //    }
    //    return true;
    //  }
    //  else
    //  {
    //    return false;
    //  }
    //}

    //public bool ConquerArea(int i, int j, int sizeOfArmy)
    //{
    //  if(IsValidMove(i, j) && !IsFriendlyArea(i, j))
    //  {
    //    if(sizeOfArmy <= 0)
    //    {
    //      return false;
    //    }

    //    int[] attackerRolls = GetRolls(sizeOfArmy, true);
    //    int[] defenderRolls = GetRolls(_board.Areas[j].SizeOfArmy, false);

    //    int losts = CompareRolls(attackerRolls, defenderRolls);

    //    if(losts > 0)
    //    {
    //      _board.Areas[j].SizeOfArmy -= losts;
    //    }
    //    else if(losts < 0)
    //    {
    //      _board.Areas[i].SizeOfArmy += losts;
    //    }
    //    else
    //    {
    //      _board.Areas[j].SizeOfArmy--;
    //      _board.Areas[i].SizeOfArmy--;
    //    }

    //    return true;
    //  }
    //  else
    //  {
    //    return false;
    //  }
    //}

    //public bool CaptureArea(int i, ArmyColor armyColor)
    //{
    //  if(IsNeutralArea(i))
    //  {
    //    _board.Areas[i].ArmyColor = armyColor;
    //    return true;
    //  }
    //  else
    //  {
    //    return false;
    //  }
    //}
  }
}
