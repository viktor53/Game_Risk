using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  public class ConfirmationEventArgs : EventArgs
  {
    public bool Data { get; private set; }

    public ConfirmationEventArgs(bool data)
    {
      Data = data;
    }
  }
}