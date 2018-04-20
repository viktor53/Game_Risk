using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal enum StatusOfGame
  {
    ROOT = -2,
    INPROGRESS = -1,
    WINNER1 = 0,
    WINNER2 = 1,
    WINNER3 = 2,
    WINNER4 = 3,
    WINNER5 = 4,
    WINNER6 = 5
  }
}