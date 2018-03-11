using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel.Game
{
  public interface IGameBoardViewModel
  {
    ViewModelBase GameDialogViewModel { get; set; }

    ArmyColor PlayerColor { get; }

    bool IsEnabled { get; set; }

    int FreeArmy { get; set; }

    Planet Selected1 { get; }

    Planet Selected2 { get; }
  }
}