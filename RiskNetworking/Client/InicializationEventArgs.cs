using Risk.Networking.Messages.Data;
using System;

namespace Risk.Networking.Client
{
  /// <summary>
  /// EventArgs when game is inicializated.
  /// </summary>
  public class InicializationEventArgs : EventArgs
  {
    /// <summary>
    /// Game board information.
    /// </summary>
    public GameBoardInfo Data { get; private set; }

    /// <summary>
    /// Creates InicializationEventArgs.
    /// </summary>
    /// <param name="data">game board informination</param>
    public InicializationEventArgs(GameBoardInfo data)
    {
      Data = data;
    }
  }
}