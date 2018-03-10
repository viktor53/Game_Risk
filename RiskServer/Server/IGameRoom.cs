using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Server
{
  public interface IGameRoom
  {
    string RoomName { get; }

    int Capacity { get; }

    int Connected { get; }

    bool AddPlayer(IClientManager player);

    bool IsFull();

    Task StartGame();

    void LeaveGame(string playerName);

    GameBoardInfo GetBoardInfo(GamePlanInfo gamePlan);

    IList<string> GetPlayers();
  }
}