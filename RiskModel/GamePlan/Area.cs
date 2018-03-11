using System.Collections.Generic;
using Risk.Model.Enums;
using Risk.Model.Units;

namespace Risk.Model.GamePlan
{
  public sealed class Area
  {
    public int ID { get; private set; }

    public int RegionID { get; private set; }

    public ArmyColor ArmyColor { get; set; }

    public int SizeOfArmy { get; set; }

    public Area(int id, int regionID)
    {
      ID = id;
      RegionID = regionID;
      ArmyColor = ArmyColor.Neutral;
      SizeOfArmy = 0;
    }
  }
}