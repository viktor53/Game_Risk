using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  enum UnitType
  {
    Infantry,
    Cannon,
    Tank
  }

  abstract class Unit
  {
    public int SizeOfArmy { get; protected set; }
    public UnitType TypeUnit { get; protected set; }
  }

  sealed class Infantry: Unit
  {
    public Infantry()
    {
      SizeOfArmy = 1;
      TypeUnit = UnitType.Infantry;
    }
  }

  sealed class Cavalary: Unit
  {
    public Cavalary()
    {
      SizeOfArmy = 5;
      TypeUnit = UnitType.Cannon;
    }
  }

  sealed class Cannon: Unit
  {
    public Cannon()
    {
      SizeOfArmy = 10;
      TypeUnit = UnitType.Tank;
    }
  }
}
