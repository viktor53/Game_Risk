using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages.Data
{
  public class GameRoomInfo
  {
    public string RoomName { get; private set; }

    public int Capacity { get; private set; }

    public int Connected { get; private set; }

    public GameRoomInfo(string roomName, int capacity, int connected)
    {
      RoomName = roomName;
      Capacity = capacity;
      Connected = connected;
    }
  }
}