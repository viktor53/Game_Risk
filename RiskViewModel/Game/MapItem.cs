using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel.Game
{
  public abstract class MapItem : ViewModelBase
  {
    public int X { get; set; }

    public int Y { get; set; }
  }
}