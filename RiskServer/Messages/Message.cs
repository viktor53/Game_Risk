using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Enums;

namespace Risk.Networking.Messages
{
  sealed class Message: IMessage
  {
    public MessageType MessageType { get; set; }

    public object Data { get; set; }

    public Message()
    {
      MessageType = MessageType.Registration;
      Data = null;
    }

    public Message(MessageType messageType, object data)
    {
      MessageType = messageType;
      Data = data;
    }
  }
}
