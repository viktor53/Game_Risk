using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Enums;

namespace Risk.Networking.Messages
{
  public interface IMessage
  {
    MessageType MessageType { get; set; }

    object Data { get; set; }
  }
}
