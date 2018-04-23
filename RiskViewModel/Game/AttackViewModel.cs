using System;
using Risk.Model.Enums;
using Risk.Networking.Client;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents attack action view model.
  /// </summary>
  public class AttackViewModel : ActionViewModelBase
  {
    private int _maxSizeOfAttack;

    /// <summary>
    /// Maximum allowed size of attack.
    /// </summary>
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

    /// <summary>
    /// Initializes attack view model and maximum size attack.
    /// </summary>
    /// <param name="gameBoardVM">game board view model</param>
    /// <param name="client">player manager that is allowed to make action</param>
    public AttackViewModel(IGameBoardViewModel gameBoardVM, IPlayer client) : base(gameBoardVM, client)
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

    /// <summary>
    /// Makes attack.
    /// </summary>
    private async void AttackClick()
    {
      await Client.SendAttackMoveAsync(GameBoardVM.PlayerColor, GameBoardVM.Selected1.ID, GameBoardVM.Selected2.ID, (AttackSize)Army);
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

        case MoveResult.AreaCaptured:
          Client.OnMoveResult -= OnMoveResult;
          GameBoardVM.GameDialogViewModel = new CaptureViewModel(GameBoardVM, Army, Client);
          break;

        case MoveResult.Winner:
          Client.OnMoveResult -= OnMoveResult;
          break;

        default:
          ErrorText = $"Move ends with error {mr}";
          break;
      }
    }
  }
}