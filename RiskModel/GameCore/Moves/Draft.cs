using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public sealed class Draft : Move
  {
    public int AreaID { get; private set; }

    public int NumberOfUnit { get; private set; }

    public Draft(ArmyColor armyColor, int areaID, int numberOfUnit) : base(armyColor)
    {
      AreaID = areaID;
      NumberOfUnit = numberOfUnit;
    }
  }
}