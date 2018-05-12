namespace Risk.Networking.Messages.Data
{
  /// <summary>
  /// Message data containing game room information.
  /// </summary>
  public class GameRoomInfo
  {
    /// <summary>
    /// Name of room.
    /// </summary>
    public string RoomName { get; private set; }

    /// <summary>
    /// Maximum player capacity.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Number of connected players.
    /// </summary>
    public int Connected { get; private set; }

    /// <summary>
    /// Creates information about game room.
    /// </summary>
    /// <param name="roomName">name of room</param>
    /// <param name="capacity">maximum capacity</param>
    /// <param name="connected">number of connected players</param>
    public GameRoomInfo(string roomName, int capacity, int connected)
    {
      RoomName = roomName;
      Capacity = capacity;
      Connected = connected;
    }
  }
}