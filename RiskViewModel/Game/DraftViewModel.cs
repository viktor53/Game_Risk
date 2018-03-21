using System;
using Risk.Model.Enums;
using Risk.Networking.Client;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents draft action veiw model.
  /// </summary>
  public class DraftViewModel : ActionViewModelBase
  {
    private int _maxSizeOfArmy;

    /// <summary>
    /// Maximum number of units to place
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
    /// Initializes draft view model.
    /// </summary>
    /// <param name="gameBoardVM">game board view model</param>
    /// <param name="client">player manager that is allowed to make action</param>
    public DraftViewModel(IGameBoardViewModel gameBoardVM, IPlayer client) : base(gameBoardVM, client)
    {
      Client.OnMoveResult += OnMoveResult;

      MaxSizeOfArmy = GameBoardVM.FreeArmy;

      Action_Click = new Command(AddArmyClick);
    }

    /// <summary>
    /// Adds army to the area.
    /// </summary>
    private async void AddArmyClick()
    {
      await Client.SendDraftMoveAsync(GameBoardVM.PlayerColor, GameBoardVM.Selected1.ID, Army);
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
          ErrorText = ErrorText = $"Move ends with error { mr}";
          break;
      }
    }
  }
}