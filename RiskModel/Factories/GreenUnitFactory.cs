using Risk.Model.Units;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  public sealed class GreenUnitFactory: UnitFactory
  {
    public GreenUnitFactory()
    {
      _infatry = new Infantry(ArmyColor.Green);
      _cavalary = new Cavalary(ArmyColor.Green);
      _cannon = new Cannon(ArmyColor.Green);
    }
  }
}
