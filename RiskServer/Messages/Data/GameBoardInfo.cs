using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Messages.Data
{
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