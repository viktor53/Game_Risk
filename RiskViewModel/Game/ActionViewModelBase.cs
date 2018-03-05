using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public abstract class ActionViewModelBase: ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVM;

    private int _army;

    public ICommand Cancel_Click { get; private set; }

    public ICommand Action_Click { get; protected set; }

    public IGameBoardViewModel GameBoardVM
    {
      get
      {
        return _gameBoardVM;
      }
    }

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

    public ActionViewModelBase(IGameBoardViewModel gameBoardVM)
    {
      _gameBoardVM = gameBoardVM;

      Army = 1;

      Cancel_Click = new Command(CancelClick);
    }

    private void CancelClick()
    {
      _gameBoardVM.GameDialogViewModel = null;
      _gameBoardVM.IsEnabled = true;
    }
  }
}
