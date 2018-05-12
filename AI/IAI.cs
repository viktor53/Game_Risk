using Risk.Model.GameCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI
{
  /// <summary>
  /// Provides contract for AI.
  /// </summary>
  public interface IAI : IPlayer
  {
    /// <summary>
    /// If player is winner.
    /// </summary>
    bool IsWinner { get; }

    /// <summary>
    /// Average time of making SetUp move.
    /// </summary>
    double GetAvgTimeSetUp { get; }

    /// <summary>
    /// Average time of making Draft move.
    /// </summary>
    double GetAvgTimeDraft { get; }

    /// <summary>
    /// Average time of making Attack move.
    /// </summary>
    double GetAvgTimeAttack { get; }

    /// <summary>
    /// Average time of making Fortify move.
    /// </summary>
    double GetAvgTimeFortify { get; }
  }
}