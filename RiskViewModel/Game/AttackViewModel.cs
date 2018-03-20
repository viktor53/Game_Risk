using Risk.Model.Enums;
using Risk.Networking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class AttackViewModel : ActionViewModelBase
  {
    private int _maxSizeOfAttack;

    public int MaxSizeOfAttack
    {
      get
      {
        return _maxSizeOfAttack;
      }
      set
      {
        _maxSizeOfAttack = value;
        OnPropertyChanged("MaxSizeOfAttack");
      }
    }

    public AttackViewModel(IGameBoardViewModel gameBoardVM, RiskClient client) : base(gameBoardVM, client)
    {
      Client.OnMoveResult += OnMoveResult;

      int max = GameBoardVM.Selected1.SizeOfArmy - 1;

      if (max > 3)
      {
        MaxSizeOfAttack = 3;
      }
      else
      {
        MaxSizeOfAttack = max;
      }

      Action_Click = new Command(AttackClick);
    }

    private void AttackClick()
    {
      Client.SendAttackMoveAsync(GameBoardVM.PlayerColor, GameBoardVM.Selected1.ID, GameBoardVM.Selected2.ID, (AttackSize)Army);
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

        case MoveResult.AreaCaptured:
          Client.OnMoveResult -= OnMoveResult;
          GameBoardVM.GameDialogViewModel = new CaptureViewModel(GameBoardVM, Army, Client);
          break;

        default:
          ErrorText = $"Move ends with error {mr}";
          break;
      }
    }
  }
}