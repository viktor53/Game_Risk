using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  enum Army
  {
    Red,
    Blue,
    Green,
    Yellow,
    Black,
    White,
    Neutral
  }

  enum MissionTarget
  {
    Conquer,
    Occupy,
    Destroy
  }

  abstract class MissionCard
  {
    public MissionTarget Target { get; protected set; }
  }

  sealed class ConquerMission: MissionCard
  {
    public Region[] Targets { get; private set; }

    public ConquerMission(Region[] targets)
    {
      Target = MissionTarget.Conquer;
      Targets = targets;
    }
  }

  sealed class OccupyMission: MissionCard
  {
    public int CountOfRegion { get; private set; }

    public OccupyMission(int countOfRegion)
    {
      Target = MissionTarget.Occupy;
      CountOfRegion = countOfRegion;
    }
  }

  sealed class DestroyMission: MissionCard
  {
    public Army Ar { get; private set; }

    public DestroyMission(Army ar)
    {
      Target = MissionTarget.Destroy;
      Ar = ar;
    }
  }
}
