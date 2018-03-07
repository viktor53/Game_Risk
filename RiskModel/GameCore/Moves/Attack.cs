using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public sealed class Attack : Move
  {
    public int AttackerAreaID { get; private set; }

    public int DefenderAreaID { get; private set; }

    public AttackSize AttackSize { get; private set; }

    public Attack(ArmyColor armyColor, int attackerAreaID, int defenderAreaID, AttackSize attackSize) : base(armyColor)
    {
      AttackerAreaID = attackerAreaID;
      DefenderAreaID = defenderAreaID;
      AttackSize = attackSize;
    }
  }
}