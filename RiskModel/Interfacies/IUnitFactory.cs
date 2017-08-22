using Risk.Model.Units;

namespace Risk.Model.Interfacies
{
  interface IUnitFactory
  {
    Infantry GetInfantryInstance();
    Cavalary GetCavalaryInstance();
    Cannon GetCannonInstance();
  }
}
