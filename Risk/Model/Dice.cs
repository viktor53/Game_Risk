using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  class Dice
  {
    private Random _ran;

    public Dice()
    {
      _ran = new Random();
    }

    public int[] RollDice()
    {
      int[] rolls = new int[3];
      rolls[0] = _ran.Next(0, 6);
      rolls[1] = _ran.Next(0, 6);
      rolls[2] = _ran.Next(0, 6);
      return rolls;
    }

    public int RollDie()
    {
      return _ran.Next(0, 6);
    }
  }
}
