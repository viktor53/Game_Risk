using Risk.Networking.Enums;

namespace Risk.Networking.Messages
{
  /// <summary>
  /// Provides contract for messages.
  /// </summary>
  public interface IMessage
  {
    /// <summary>
    /// Type of message.
    /// </summary>
    MessageType MessageType { get; }

    /// <summary>
    /// Data of message.
    /// </summary>
    object Data { get; }
  }
}