using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  public class UpdateGameEventArgs : EventArgs
  {
    public Area Data { get; private set; }

    public UpdateGameEventArgs(Area data)
    {
      Data = data;
    }
  }
}