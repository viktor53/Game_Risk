using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  class Area
  {
    public int ID { get; private set; }

    public Region Reg { get; private set; }

    public Army Ar { get; set; }

    public List<Unit> Units { get; private set; }

    public Area(int id, Region reg)
    {
      ID = id;
      Reg = reg;
      Ar = Army.Neutral;
      Units = new List<Unit>();
    }
  }

  class GameBoard
  {
    public bool[][] Board { get; private set; }

    public Area[] Areas { get; private set; }

    public GameBoard(int countOfAreas)
    {
      Board = new bool[countOfAreas][];
      for(int i = 0; i < countOfAreas; i++)
      {
        Board[i] = new bool[countOfAreas];
      }

      Areas = new Area[countOfAreas];
    }
  }
}
