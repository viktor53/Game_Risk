using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class AttackViewModel: ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVW;
    
    private int _army;

    public ICommand Cancel_Click { get; private set; }

    public ICommand Attack_Click { get; private set; }

    public int Army
    {
      get
      {
        return _army;
      }
      set
      {
        _army = value;
        OnPropertyChanged("Army");
      }
    }

    public AttackViewModel(IGameBoardViewModel gameBoardVW)
    {
      _gameBoardVW = gameBoardVW;

      Army = 1;

      Cancel_Click = new Command(CancelClick);
      Attack_Click = new Command(AttackClick);
    }

    private void CancelClick()
    {
      _gameBoardVW.GameDialogViewModel = null;
      _gameBoardVW.IsEnabled = true;
    }

    private void AttackClick()
    {

    }
  }
}
