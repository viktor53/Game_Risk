using System;
using Risk.Model.Enums;
using Risk.Networking.Client;

namespace Risk.ViewModel.Game
{
  public class FortifyViewModel : ActionViewModelBase
  {
    private int _maxSizeOfArmy;

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
    /// Initializes fortify view model and sets maximum size of army.
    /// </summary>
    /// <param name="gameBoardVM">game board view model</param>
    /// <param name="client">player manager that is allowed to make action</param>
    public FortifyViewModel(IGameBoardViewModel gameBoardVM, RiskClient client) : base(gameBoardVM, client)
    {
      Client.OnMoveResult += OnMoveResult;

      MaxSizeOfArmy = GameBoardVM.Selected1.SizeOfArmy - 1;

      Action_Click = new Command(MoveArmyClick);
    }

    /// <summary>
    /// Moves army to the area.
    /// </summary>
    private async void MoveArmyClick()
    {
      await Client.SendFortifyMoveAsync(GameBoardVM.PlayerColor, GameBoardVM.Selected1.ID, GameBoardVM.Selected2.ID, Army);
    }

    /// <summary>
    /// Method is called when OnMoveResult event is raised. Processes move result.
    /// </summary>
    /// <param name="sender">sender who raised the event</param>
    /// <param name="ev">MoveResultEventArgs</param>
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