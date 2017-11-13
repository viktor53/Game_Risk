using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Enums
{
  public enum MessageType
  {
    Registration = 0,
    Update = 1,
    UpdateDifference = 2,
    Logout = 3,
    ConnectToGame = 4,
    CreateGame = 5,
    ReadyTag = 6,
    Move = 7,
    Confirmation = 8,
    Error = 9
  }
}
