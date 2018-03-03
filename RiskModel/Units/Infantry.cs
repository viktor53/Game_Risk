﻿using Risk.Model.Enums;

namespace Risk.Model.Units
{
  public sealed class Infantry : Unit
  {
    internal Infantry(ArmyColor armyColor)
    {
      SizeOfArmy = 1;
      TypeUnit = UnitType.Infantry;
      ArmyColor = armyColor;
    }
  }
}
