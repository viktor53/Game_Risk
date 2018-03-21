using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Represent view model with information if player won.
  /// </summary>
  public class WinnerViewModel : ViewModelBase
  {
    private string _text;

    private IWindowManager _windowManager;

    private ViewModelBase _viewModel;

    /// <summary>
    /// Click on OK button. Returns to menu.
    /// </summary>
    public ICommand OK_Click { get; private set; }

    /// <summary>
    /// Text about winning.
    /// </summary>
    public string Text
    {
      get
      {
        return _text;
      }
      set
      {
        _text = value;
        OnPropertyChanged("Text");
      }
    }

    /// <summary>
    /// Initializes WinnerViewModel with information if player won.
    /// </summary>
    /// <param name="windowManager">window manager</param>
    /// <param name="menu">menu where player is returned</param>
    /// <param name="isWinner">if player won</param>
    public WinnerViewModel(IWindowManager windowManager, ViewModelBase menu, bool isWinner)
    {
      _viewModel = menu;
      _windowManager = windowManager;

      OK_Click = new Command(OKClick);

      if (isWinner)
      {
        Text = "You Win!";
      }
      else
      {
        Text = "You lose!";
      }
    }

    /// <summary>
    /// Returns to menu.
    /// </summary>
    private void OKClick()
    {
      _windowManager.WindowViewModel = _viewModel;
    }
  }
}