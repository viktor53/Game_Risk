using Risk.Model.GamePlan;
using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when area of game board is changed.
  /// </summary>
  public class UpdateGameEventArgs : EventArgs
  {
    /// <summary>
    /// Changed area.
    /// </summary>
    public Area Data { get; private set; }

    /// <summary>
    /// Creates UpdateGameEventArgs.
    /// </summary>
    /// <param name="data">changed area</param>
    public UpdateGameEventArgs(Area data)
    {
      Data = data;
    }
  }
}