using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk
{

  enum Region
  {
    Australie,
    NorthAmerica,
    SouthAmerica,
    Africa,
    Asia,
    Europe
  }

  class RiskCard
  {
    public UnitType TypeUnit { get; private set; }

    public Region Reg { get; private set; }

    public RiskCard(UnitType type, Region reg)
    {
      TypeUnit = type;
      Reg = reg;
    }
  }
}
