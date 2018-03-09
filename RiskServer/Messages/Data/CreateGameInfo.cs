using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages.Data
{
  public class CreateGameRoomInfo
  {
    public string RoomName { get; private set; }

    public int Capacity { get; private set; }

    public bool IsClassic { get; private set; }

    public CreateGameRoomInfo(string roomName, int capacity, bool isClassic)
    {
      RoomName = roomName;
      Capacity = capacity;
      IsClassic = isClassic;
    }
  }
}