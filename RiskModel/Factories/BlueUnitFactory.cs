using Risk.Model.Units;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  sealed class BlueUnitFactory: UnitFactory
  {
    public BlueUnitFactory()
    {
      _infatry = new Infantry(ArmyColor.Blue);
      _cavalary = new Cavalary(ArmyColor.Blue);
      _cannon = new Cannon(ArmyColor.Blue);
    }
  }
}
