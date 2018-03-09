using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;

namespace Risk.Model.GameCore
{
  public interface IGame
  {
    void StartGame();

    bool AddPlayer(IPlayer player);

    GamePlanInfo GetGamePlan();

    MoveResult MakeMove(SetUp move);

    MoveResult MakeMove(Draft move);

    MoveResult MakeMove(Attack move);

    MoveResult MakeMove(Fortify move);

    MoveResult MakeMove(Capture move);

    MoveResult MakeMove(ExchangeCard move);
  }
}