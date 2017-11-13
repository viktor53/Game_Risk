using Risk.Model.Units;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  public sealed class BlackUnitFactory: UnitFactory
  {
    public BlackUnitFactory()
    {
      _infatry = new Infantry(ArmyColor.Black);
      _cavalary = new Cavalary(ArmyColor.Black);
      _cannon = new Cannon(ArmyColor.Black);
    }
  }
}
