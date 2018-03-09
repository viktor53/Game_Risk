using Risk.Model.GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Server
{
  public interface IClientManager : IPlayer
  {
    string PlayerName { get; }

    IGameRoom GameRoom { get; set; }

    Task SendNewPlayerConnected(string name);

    Task SendPlayerLeave(string name);

    Task SendConnectedPlayers(IList<string> players);

    Task<bool> WaitUntilPlayerIsReady();

    Task SartListening();

    Task SendUpdateGameList(IList<GameRoomInfo> roomsInfo);
  }
}