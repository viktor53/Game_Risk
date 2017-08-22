using Risk.Model.Units;
using Risk.Model.Enums;
using Risk.Model.Interfacies;


namespace Risk.Model.Factories
{
  internal abstract class UnitFactory: IUnitFactory
  {
    protected Infantry _infatry;
    protected Cavalary _cavalary;
    protected Cannon _cannon;

    public Infantry GetInfantryInstance()
    {
      return _infatry;
    }

    public Cavalary GetCavalaryInstance()
    {
      return _cavalary;
    }

    public Cannon GetCannonInstance()
    {
      return _cannon;
    }
  }
}
