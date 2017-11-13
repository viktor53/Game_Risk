using Risk.Model.Units;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  public sealed class WhiteUnitFactory: UnitFactory
  {
    public WhiteUnitFactory()
    {
      _infatry = new Infantry(ArmyColor.White);
      _cavalary = new Cavalary(ArmyColor.White);
      _cannon = new Cannon(ArmyColor.White);
    }
  }
}
