using Risk.Model.GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI
{
  public interface IAI : IPlayer
  {
    bool IsWinner { get; }

    double GetAvgTimeSetUp { get; }

    double GetAvgTimeDraft { get; }

    double GetAvgTimeAttack { get; }

    double GetAvgTimeFortify { get; }
  }
}