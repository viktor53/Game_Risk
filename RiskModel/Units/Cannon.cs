using Risk.Model.Enums;

namespace Risk.Model.Units
{
  public sealed class Cannon : Unit
  {
    internal Cannon(ArmyColor armyColor)
    {
      SizeOfArmy = 10;
      TypeUnit = UnitType.Cannon;
      ArmyColor = armyColor;
    }
  }
}
