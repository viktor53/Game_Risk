using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk
{
  enum UnitType
  {
    Infantry,
    Cannon,
    Tank
  }

  abstract class Unit
  {
    public UnitType TypeUnit { get; protected set; }
  }

  sealed class Infantry: Unit
  {
    public Infantry()
    {
      TypeUnit = UnitType.Infantry;
    }
  }

  sealed class Cannon: Unit
  {
    public Cannon()
    {
      TypeUnit = UnitType.Cannon;
    }
  }

  sealed class Tank: Unit
  {
    public Tank()
    {
      TypeUnit = UnitType.Tank;
    }
  }
}
