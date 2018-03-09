using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Enums
{
  public enum ErrorType
  {
    UknownRequest = 0,
    NameExist = 1,
    GameNameExist = 2,
    GameIsFull = 3
  }
}