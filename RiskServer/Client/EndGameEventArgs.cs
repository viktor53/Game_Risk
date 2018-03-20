using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when game ended.
  /// </summary>
  public class EndGameEventArgs : EventArgs
  {
    /// <summary>
    /// if the player is winner or not.
    /// </summary>
    public bool Data { get; private set; }

    /// <summary>
    /// Creates EndGameEventArgs.
    /// </summary>
    /// <param name="data">if the player is winner</param>
    public EndGameEventArgs(bool data)
    {
      Data = data;
    }
  }
}