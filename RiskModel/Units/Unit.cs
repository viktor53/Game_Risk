using Risk.Model.Enums;

namespace Risk.Model.Units
{
  public abstract class Unit
  {
    public ArmyColor ArmyColor { get; protected set; }
    public int SizeOfArmy { get; protected set; }
    public UnitType TypeUnit { get; protected set; }
  }
}
