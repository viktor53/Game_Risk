using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages
{
  public interface IRequestFactory
  {
    IMessage CreateRegistrationRequest(string username);

    IMessage CreateUpdateRequest(string username);

    IMessage CreateConnectToGameRequest(string username, string gameName);

    IMessage CreateCreateGameRequest(string username, string gameName, int numberOfPlayers);

    IMessage CreateLogoutRequest(string username);

    IMessage CreateReadyRequest(string username, string gameName);

    IMessage CreateMoveRequest(string username, string gameName);
  }
}
