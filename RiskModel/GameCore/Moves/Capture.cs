using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public sealed class Capture : Move
  {
    public int ArmyToMove { get; private set; }

    public Capture(ArmyColor armyColor, int armyToMove) : base(armyColor)
    {
      ArmyToMove = armyToMove;
    }
  }
}