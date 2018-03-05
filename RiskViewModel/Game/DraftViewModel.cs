using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class DraftViewModel: ActionViewModelBase
  {
    public DraftViewModel(IGameBoardViewModel gameBoardVM): base(gameBoardVM)
    {
      Action_Click = new Command(AddArmyClick);
    }

    private void AddArmyClick()
    {

    }
  }
}
