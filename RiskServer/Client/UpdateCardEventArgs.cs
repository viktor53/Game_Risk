using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when number of cards is changed.
  /// </summary>
  public class UpdateCardEventArgs : EventArgs
  {
    /// <summary>
    /// If card was added or removed.
    /// </summary>
    public bool Data { get; private set; }

    /// <summary>
    /// Creates UpdateCardEventArgs.
    /// </summary>
    /// <param name="data">if card was added or removed</param>
    public UpdateCardEventArgs(bool data)
    {
      Data = data;
    }
  }
}