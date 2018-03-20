using Risk.Networking.Enums;

namespace Risk.Networking.Messages
{
  /// <summary>
  /// Represents a message of communication between server and client.
  /// </summary>
  internal sealed class Message : IMessage
  {
    /// <summary>
    /// Type of message.
    /// </summary>
    public MessageType MessageType { get; private set; }

    /// <summary>
    /// Data of message.
    /// </summary>
    public object Data { get; private set; }

    /// <summary>
    /// Creates message of the type.
    /// </summary>
    /// <param name="messageType">type of message</param>
    /// <param name="data">data of message</param>
    public Message(MessageType messageType, object data)
    {
      MessageType = messageType;
      Data = data;
    }
  }
}