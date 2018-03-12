using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  public class ArmyColorEventArgs : EventArgs
  {
    public long Data { get; private set; }

    public ArmyColorEventArgs(long data)
    {
      Data = data;
    }
  }
}