using System;
using Risk.Model.Enums;
using Risk.Networking.Client;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents capture action view model.
  /// </summary>
  public class CaptureViewModel : ActionViewModelBase
  {
    private int _maxSizeOfArmy;

    private int _minSizeOfArmy;

    /// <summary>
    /// Maximum number of units to move.
    /// </summary>
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

    /// <summary>
    /// Minimum number of units to move.
    /// </summary>
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

    /// <summary>
    /// Initializes capture view model and max/min size of army.
    /// </summary>
    /// <param name="gameBoardVM">game board view model</param>
    /// <param name="attackSize">size of attack that captured the area</param>
    /// <param name="client">player manager that is allowed to make action</param>
    public CaptureViewModel(IGameBoardViewModel gameBoardVM, int attackSize, IPlayer client) : base(gameBoardVM, client)
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

    /// <summary>
    /// Moves units to captured area.
    /// </summary>
    private async void MoveClick()
    {
      await Client.SendCaptureMoveAsync(GameBoardVM.PlayerColor, Army);
    }

    /// <summary>
    /// Method that is called when OnMoveResult event is raised. Processes move result.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
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