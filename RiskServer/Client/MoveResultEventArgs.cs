using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  public class MoveResultEventArgs : EventArgs
  {
    public long Data { get; private set; }

    public MoveResultEventArgs(long data)
    {
      Data = data;
    }
  }
}