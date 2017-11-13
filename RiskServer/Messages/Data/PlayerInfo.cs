using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages.Data
{
  public sealed class PlayerInfo
  {
    public string Username { get; set; }

    public bool IsInGame { get; set; }
  }
}
