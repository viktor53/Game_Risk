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

    bool IsEnabled { get; set; }

    int FreeArmy { get; set; }
  }
}
