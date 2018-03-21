using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represents error view model with specific error message.
  /// </summary>
  public class ErrorViewModel : ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVM;

    private string _errorText;

    /// <summary>
    /// Click on OK button. Closes the error view model.
    /// </summary>
    public ICommand OK_Click { get; private set; }

    /// <summary>
    /// Error message.
    /// </summary>
    public string ErrorText
    {
      get
      {
        return _errorText;
      }
      set
      {
        _errorText = value;
        OnPropertyChanged("ErrorText");
      }
    }

    /// <summary>
    /// Initializes ErrorViewModel with specific error message.
    /// </summary>
    /// <param name="gameBoardVM">game board view model</param>
    /// <param name="error">error message</param>
    public ErrorViewModel(IGameBoardViewModel gameBoardVM, string error)
    {
      _gameBoardVM = gameBoardVM;
      ErrorText = error;

      OK_Click = new Command(OKClick);
    }

    /// <summary>
    /// Closes error view model.
    /// </summary>
    private void OKClick()
    {
      _gameBoardVM.IsEnabled = true;
      _gameBoardVM.GameDialogViewModel = null;
    }
  }
}