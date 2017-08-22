using Risk.Model.Enums;

namespace Risk.Model.Units
{
  sealed class Cavalary : Unit
  {
    internal Cavalary(ArmyColor armyColor)
    {
      SizeOfArmy = 5;
      TypeUnit = UnitType.Cavalary;
      ArmyColor = armyColor;
    }
  }
}
