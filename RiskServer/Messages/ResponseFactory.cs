using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;
using Risk.Networking.Enums;

namespace Risk.Networking.Messages
{
  sealed class ResponseFactory : IResponseFactory
  {
    private Message _message;

    public ResponseFactory()
    {
      _message = new Message();
    }

    public IMessage CreateConfirmationResponse(bool result)
    {
      _message.MessageType = MessageType.Confirmation;
      _message.Data = result;
      return _message;
    }

    public IMessage CreateErrorResponse(Error error)
    {
      _message.MessageType = MessageType.Error;
      _message.Data = error;
      return _message;
    }

    public IMessage CreateUpdateDifference(UpdateDifference updateDifference)
    {
      _message.MessageType = MessageType.UpdateDifference;
      _message.Data = updateDifference;
      return _message;
    }

    public IMessage CreateUpdateResponse(UpdateInfo updateInfo)
    {
      _message.MessageType = MessageType.Update;
      _message.Data = updateInfo;
      return _message;
    }
  }
}
