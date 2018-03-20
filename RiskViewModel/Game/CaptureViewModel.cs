using Risk.Model.Enums;
using Risk.Networking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.ViewModel.Game
{
  public class CaptureViewModel : ActionViewModelBase
  {
    private int _maxSizeOfArmy;

    private int _minSizeOfArmy;

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

    public int MinSizeOfArmy
    {
      get
      {
        return _minSizeOfArmy;
      }
      set
      {
        _minSizeOfArmy = value;
        OnPropertyChanged("MinSizeOfArmy");
      }
    }

    public CaptureViewModel(IGameBoardViewModel gameBoardVM, int attackSize, RiskClient client) : base(gameBoardVM, client)
    {
      Client.OnMoveResult += OnMoveResult;

      MaxSizeOfArmy = gameBoardVM.Selected1.SizeOfArmy - 1;

      MinSizeOfArmy = attackSize;

      if (MaxSizeOfArmy == MinSizeOfArmy)
      {
        Army = MaxSizeOfArmy;
      }

      Action_Click = new Command(MoveClick);
    }

    private void MoveClick()
    {
      Client.SendCaptureMoveAsync(GameBoardVM.PlayerColor, Army);
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