using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  class MoveController
  {
    private GameBoard _board;

    public MoveController(GameBoard board)
    {
      _board = board;
    }

    private bool IsFriendlyArea(int i, int j)
    {
      return _board.Areas[i].Ar == _board.Areas[j].Ar;
    }

    private bool IsValidMove(int i, int j)
    {
      return _board.Board[i][j];
    }

    private bool IsNeutralArea(int i)
    {
      return _board.Areas[i].Ar == Army.Neutral;
    }

    public bool MoveUnit(int i, int j, IList<Unit> units)
    {
      if(IsValidMove(i, j) && IsFriendlyArea(i, j))
      {
        foreach(Unit unit in units)
        {
          _board.Areas[j].Units.Add(unit);
          _board.Areas[i].Units.Remove(unit);
        }
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool ConquerArea(int i, int j, IList<Unit> units)
    {
      if(IsValidMove(i, j) && !IsFriendlyArea(i, j))
      {

        return true;
      }
      else
      {
        return false;
      }
    }

    public bool CaptureArea(int i, Army armyColor)
    {
      if(IsNeutralArea(i))
      {
        _board.Areas[i].Ar = armyColor;
        return true;
      }
      else
      {
        return false;
      }
    }
  }
}
