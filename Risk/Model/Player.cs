using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  class Player
  {
    private int _sizeOfArmy;

    public Army ArmyColor { get; private set; }

    public int SizeOfArmy { get
      {
        return _sizeOfArmy;
      }
      set
      {
        if(value >= 42)
        {
          _sizeOfArmy = 42;
        }
      }
    }

    public Player(Army armyColor, int sizeOfArmy)
    {
      ArmyColor = armyColor;
      _sizeOfArmy = sizeOfArmy;
    }

  }
}
