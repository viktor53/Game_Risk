using Risk.Model.Units;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  sealed class RedUnitFactory : UnitFactory
  {
    public RedUnitFactory()
    {
      _infatry = new Infantry(ArmyColor.Red);
      _cavalary = new Cavalary(ArmyColor.Red);
      _cannon = new Cannon(ArmyColor.Red);
    }
  }
}
