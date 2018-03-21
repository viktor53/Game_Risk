using System.Windows.Input;

namespace Risk.ViewModel.Multiplayer
{
  /// <summary>
  /// Represents connect to game error dialog.
  /// </summary>
  public class ConnectToGameErrorDialogViewModel : ViewModelBase
  {
    private IMultiplayerMenuViewModel _multiplayerViewModel;

    /// <summary>
    /// Click on OK button. Returns to multiplayer menu.
    /// </summary>
    public ICommand OK_Click { get; private set; }

    /// <summary>
    /// Initializes ConnectToGameErrorDialogViewModel.
    /// </summary>
    /// <param name="multiplayerViewModel"></param>
    public ConnectToGameErrorDialogViewModel(IMultiplayerMenuViewModel multiplayerViewModel)
    {
      _multiplayerViewModel = multiplayerViewModel;

      OK_Click = new Command(OKClick);
    }

    /// <summary>
    /// Returns to multiplayer menu.
    /// </summary>
    private void OKClick()
    {
      _multiplayerViewModel.DialogViewModel = null;
      _multiplayerViewModel.IsEnabled = true;
    }
  }
}