using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages.Data
{
  public sealed class UpdateInfo
  {
    public ICollection<GameInfo> Games { get; set; }

    public ICollection<PlayerInfo> Players { get; set; }
  }
}
