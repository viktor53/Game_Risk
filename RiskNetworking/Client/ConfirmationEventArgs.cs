using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when confirmation occurs.
  /// </summary>
  public class ConfirmationEventArgs : EventArgs
  {
    /// <summary>
    /// If it is OK or not.
    /// </summary>
    public bool Data { get; private set; }

    /// <summary>
    /// Creates ConfiramtionEventArgs.
    /// </summary>
    /// <param name="data">confiramtion yes/no</param>
    public ConfirmationEventArgs(bool data)
    {
      Data = data;
    }
  }
}