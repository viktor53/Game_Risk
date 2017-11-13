using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages.Data
{
  public sealed class GameInfo
  {
    public string GameName { get; set; }

    public int Capacity { get; set; }

    public int Registred { get; set; }

    public IList<string> Players { get; set; }
  }
}
