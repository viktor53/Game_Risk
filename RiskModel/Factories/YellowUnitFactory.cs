using Risk.Model.Units;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  public sealed class YellowUnitFactory: UnitFactory
  {
    public YellowUnitFactory()
    {
      _infatry = new Infantry(ArmyColor.Yellow);
      _cavalary = new Cavalary(ArmyColor.Yellow);
      _cannon = new Cannon(ArmyColor.Yellow);
    }
  }
}
