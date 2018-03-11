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
  public class DraftViewModel : ActionViewModelBase
  {
    public DraftViewModel(IGameBoardViewModel gameBoardVM, RiskClient client) : base(gameBoardVM, client)
    {
      Client.OnMoveResult += OnMoveResult;

      Action_Click = new Command(AddArmyClick);
    }

    private void AddArmyClick()
    {
      Client.SendDraftMove(GameBoardVM.PlayerColor, GameBoardVM.Selected1.ID, Army);
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
          ErrorText = ErrorText = $"Move ends with error { mr}";
          break;
      }
    }
  }
}