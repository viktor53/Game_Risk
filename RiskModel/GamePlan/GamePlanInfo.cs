using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GamePlan
{
  public class GamePlanInfo
  {
    public bool[][] Connections { get; private set; }

    public Area[] Areas { get; private set; }

    public GamePlanInfo(bool[][] connections, Area[] areas)
    {
      Connections = connections;
      Areas = areas;
    }
  }
}