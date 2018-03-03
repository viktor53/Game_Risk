using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class DraftViewModel: ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVW;

    private int _army;

    public ICommand Cancel_Click { get; private set; }

    public ICommand AddArmy_Click { get; private set; }

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

    public DraftViewModel(IGameBoardViewModel gameBoardVW)
    {
      _gameBoardVW = gameBoardVW;

      Cancel_Click = new Command(CancelClick);
      AddArmy_Click = new Command(AddArmyClick);
    }

    public IGameBoardViewModel GameBoardVM
    {
      get
      {
        return _gameBoardVW;
      }
    }

    private void CancelClick()
    {
      _gameBoardVW.GameDialogViewModel = null;
      _gameBoardVW.IsEnabled = true;
    }

    private void AddArmyClick()
    {

    }
  }
}
