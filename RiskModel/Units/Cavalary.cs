using Risk.Model.Enums;

namespace Risk.Model.Units
{
  public sealed class Cavalary : Unit
  {
    internal Cavalary(ArmyColor armyColor)
    {
      SizeOfArmy = 5;
      TypeUnit = UnitType.Cavalary;
      ArmyColor = armyColor;
    }
  }
}
