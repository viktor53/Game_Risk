﻿using System.Collections.Generic;
using Risk.Model.Enums;
using Risk.Model.Units;

namespace Risk.Model.GamePlan
{
  class Area
  {
    public int ID { get; private set; }

    public Region Reg { get; private set; }

    public ArmyColor ArmyColor { get; set; }

    public int SizeOfArmy { get; set; }

    public List<Unit> Units { get; private set; }

    public Area(int id, Region reg)
    {
      ID = id;
      Reg = reg;
      ArmyColor = ArmyColor.Neutral;
      SizeOfArmy = 0;
      Units = new List<Unit>();
    }
  }
}
