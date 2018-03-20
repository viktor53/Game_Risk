using Risk.Model.Enums;
using Risk.Networking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class FortifyViewModel : ActionViewModelBase
  {
    private int _maxSizeOfArmy;

    public int MaxSizeOfArmy
    {
      get
      {
        return _maxSizeOfArmy;
      }
      set
      {
        _maxSizeOfArmy = value;
        OnPropertyChanged("MaxSizeOfArmy");
      }
    }

    public FortifyViewModel(IGameBoardViewModel gameBoardVM, RiskClient client) : base(gameBoardVM, client)
    {
      Client.OnMoveResult += OnMoveResult;

      MaxSizeOfArmy = GameBoardVM.Selected1.SizeOfArmy - 1;

      Action_Click = new Command(MoveArmyClick);
    }

    private void MoveArmyClick()
    {
      Client.SendFortifyMoveAsync(GameBoardVM.PlayerColor, GameBoardVM.Selected1.ID, GameBoardVM.Selected2.ID, Army);
    }

    private void OnMoveResult(object sender, EventArgs ev)
    {
      MoveResult mr = (MoveResult)((MoveResultEventArgs)ev).Data;
      switch (mr)
      {
        case MoveResult.OK:
          Client.OnMoveResult -= OnMoveResult;
          GameBoardVM.GameDialogViewModel = null;
          break;

        default:
          ErrorText = $"Move ends with error {mr}";
          break;
      }
    }
  }
}