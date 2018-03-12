using Risk.Networking.Messages.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  public class InicializationEventArgs : EventArgs
  {
    public GameBoardInfo Data { get; private set; }

    public InicializationEventArgs(GameBoardInfo data)
    {
      Data = data;
    }
  }
}