using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel.Game
{
  public sealed class Connection : MapItem
  {
    public int X2 { get; set; }

    public int Y2 { get; set; }

    public Connection(int x, int y, int x2, int y2)
    {
      X = x;
      Y = y;
      X2 = x2;
      Y2 = y2;
    }
  }
}