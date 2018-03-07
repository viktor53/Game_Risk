using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public sealed class Fortify : Move
  {
    public int FromAreaID { get; private set; }

    public int ToAreaID { get; private set; }

    public int SizeOfArmy { get; private set; }

    public Fortify(ArmyColor armyColor, int fromAreaID, int toAreaID, int sizeOfArmy) : base(armyColor)
    {
      FromAreaID = fromAreaID;
      ToAreaID = toAreaID;
      SizeOfArmy = sizeOfArmy;
    }
  }
}