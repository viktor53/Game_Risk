using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class FortifyViewModel: ActionViewModelBase
  {
    public FortifyViewModel(IGameBoardViewModel gameBoardVM): base(gameBoardVM)
    {
      Action_Click = new Command(MoveArmyClick);
    }

    private void MoveArmyClick()
    {

    }
  }
}
