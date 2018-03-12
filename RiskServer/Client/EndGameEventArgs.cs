using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  public class EndGameEventArgs : EventArgs
  {
    public bool Data { get; private set; }

    public EndGameEventArgs(bool data)
    {
      Data = data;
    }
  }
}