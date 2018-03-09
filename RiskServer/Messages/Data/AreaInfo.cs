using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.GamePlan;

namespace Risk.Networking.Messages.Data
{
  public class AreaInfo
  {
    public Coordinates Position { get; private set; }

    public Area Area { get; private set; }

    public AreaInfo(Coordinates position, Area area)
    {
      Position = position;
      Area = area;
    }
  }
}