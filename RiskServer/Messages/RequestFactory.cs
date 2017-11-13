using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Enums;

namespace Risk.Networking.Messages
{
  sealed class RequestFactory : IRequestFactory
  {
    private Message _message;

    public RequestFactory()
    {
      _message = new Message();
    }

    public IMessage CreateConnectToGameRequest(string username, string gameName)
    {
      throw new NotImplementedException();
    }

    public IMessage CreateCreateGameRequest(string username, string gameName, int numberOfPlayers)
    {
      throw new NotImplementedException();
    }

    public IMessage CreateLogoutRequest(string username)
    {
      _message.MessageType = MessageType.Logout;
      _message.Data = username;
      return _message;
    }

    public IMessage CreateMoveRequest(string username, string gameName)
    {
      throw new NotImplementedException();
    }

    public IMessage CreateReadyRequest(string username, string gameName)
    {
      throw new NotImplementedException();
    }

    public IMessage CreateRegistrationRequest(string username)
    {
      _message.MessageType = MessageType.Registration;
      _message.Data = username;
      return _message;
    }

    public IMessage CreateUpdateRequest(string username)
    {
      _message.MessageType = MessageType.Update;
      _message.Data = username;
      return _message;
    }
  }
}
