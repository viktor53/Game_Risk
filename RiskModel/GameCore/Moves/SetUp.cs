using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public sealed class SetUp : Move
  {
    public int AreaID { get; private set; }

    public SetUp(ArmyColor armyColor, int areaID) : base(armyColor)
    {
      AreaID = areaID;
    }
  }
}