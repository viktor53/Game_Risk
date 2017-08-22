using Risk.Model.Enums;

namespace Risk.Model.Interfacies
{
  interface IPlayer
  {
    void RiseArmy(int countOfArmy);

    ArmyColor GetColor();
  }
}
