using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when move result occurs.
  /// </summary>
  public class MoveResultEventArgs : EventArgs
  {
    /// <summary>
    /// Move result.
    /// </summary>
    public long Data { get; private set; }

    /// <summary>
    /// Creates MoveResultEventArgs.
    /// </summary>
    /// <param name="data">move result</param>
    public MoveResultEventArgs(long data)
    {
      Data = data;
    }
  }
}