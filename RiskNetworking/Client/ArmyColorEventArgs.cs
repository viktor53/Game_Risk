using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when color of player is changed.
  /// </summary>
  public class ArmyColorEventArgs : EventArgs
  {
    /// <summary>
    /// New color of player.
    /// </summary>
    public long Data { get; private set; }

    /// <summary>
    /// Creates ArmyColorEventArgs.
    /// </summary>
    /// <param name="data">new color of player</param>
    public ArmyColorEventArgs(long data)
    {
      Data = data;
    }
  }
}