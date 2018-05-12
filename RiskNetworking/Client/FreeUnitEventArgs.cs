using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when number of free units was changed.
  /// </summary>
  public class FreeUnitEventArgs : EventArgs
  {
    /// <summary>
    /// Number of free units.
    /// </summary>
    public long Data { get; private set; }

    /// <summary>
    /// Creates FreeUnitEventArgs.
    /// </summary>
    /// <param name="data">number of free units</param>
    public FreeUnitEventArgs(long data)
    {
      Data = data;
    }
  }
}