using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.GamePlan;

namespace Risk.Networking.Server
{
  public class AreaInfo
  {
    public int X { get; private set; }

    public int Y { get; private set; }

    public Area Area { get; private set; }

    public AreaInfo(int x, int y, Area area)
    {
      X = x;
      Y = y;
      Area = area;
    }
  }

  public class GameBoardInfo
  {
    public IList<IList<bool>> Connections { get; private set; }

    public List<AreaInfo> AreaInfos { get; private set; }

    public GameBoardInfo(bool[][] con, List<AreaInfo> areaInfos)
    {
      Connections = con;
      AreaInfos = areaInfos;
    }
  }
}
