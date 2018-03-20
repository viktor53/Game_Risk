using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages.Data
{
  /// <summary>
  /// Message data representing information about creating new game room.
  /// </summary>
  public class CreateGameRoomInfo
  {
    /// <summary>
    /// Name of new room.
    /// </summary>
    public string RoomName { get; private set; }

    /// <summary>
    /// Maximum player capacity of new room.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Is it classic map or random generating map.
    /// </summary>
    public bool IsClassic { get; private set; }

    /// <summary>
    /// Creates information about creating new game room.
    /// </summary>
    /// <param name="roomName">name of new room</param>
    /// <param name="capacity">maximum capacity</param>
    /// <param name="isClassic">classic map or random generating map</param>
    public CreateGameRoomInfo(string roomName, int capacity, bool isClassic)
    {
      RoomName = roomName;
      Capacity = capacity;
      IsClassic = isClassic;
    }
  }
}