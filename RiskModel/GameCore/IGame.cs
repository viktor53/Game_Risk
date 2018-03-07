using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;

namespace Risk.Model.GameCore
{
  internal interface IGame
  {
    void StartGame();

    bool AddPlayer(IPlayer player);

    MoveResult MakeMove(SetUp move);

    MoveResult MakeMove(Draft move);

    MoveResult MakeMove(Attack move);

    MoveResult MakeMove(Fortify move);

    MoveResult MakeMove(Capture move);

    MoveResult MakeMove(ExchangeCard move);
  }
}