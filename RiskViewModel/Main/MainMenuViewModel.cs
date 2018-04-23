using System.Windows.Input;

namespace Risk.ViewModel.Main
{
  /// <summary>
  /// Represents main menu view model.
  /// </summary>
  public class MainMenuViewModel : ViewModelBase, IMainMenuViewModel
  {
    private ViewModelBase _contentViewModel;

    private IWindowManager _windowManager;

    private bool _isEnabled = true;

    /// <summary>
    /// Click on Multiplayer button. Changes ContentViewModel on ConnectionViewModel.
    /// </summary>
    public ICommand Multiplayer_Click { get; private set; }

    /// <summary>
    /// Click on Singleplayer button. Changes ContentViewModel on IntroSinglePlayerVewModel.
    /// </summary>
    public ICommand Singleplayer_Click { get; private set; }

    /// <summary>
    /// Click on QuitGame button. Closes the application.
    /// </summary>
    public ICommand QuitGame_Click { get; private set; }

    /// <summary>
    /// View model for content of main menu.
    /// </summary>
    public ViewModelBase ContentViewModel
    {
      get
      {
        return _contentViewModel;
      }

      set
      {
        _contentViewModel = value;
        this.OnPropertyChanged("ContentViewModel");
      }
    }

    /// <summary>
    /// If main menu is enabled.
    /// </summary>
    public bool IsEnabled
    {
      get
      {
        return _isEnabled;
      }
      set
      {
        _isEnabled = value;
        OnPropertyChanged("IsEnabled");
      }
    }

    /// <summary>
    /// Initializes MainMenuVeiwModel.
    /// </summary>
    /// <param name="windowManager">window manager</param>
    public MainMenuViewModel(IWindowManager windowManager)
    {
      Multiplayer_Click = new Command(MultiplayerClick);

      Singleplayer_Click = new Command(SingleplayerClick);

      QuitGame_Click = new Command(QuitGameClick);

      _windowManager = windowManager;
    }

    /// <summary>
    /// Changes ContentViewModel on IntroSignlePLayerViewModel.
    /// </summary>
    private void SingleplayerClick()
    {
      ContentViewModel = new CreateSinglePlayerViewModel(_windowManager, this);
    }

    /// <summary>
    /// Changes ContentViewModel on ConnectionViewModel and creates RiskClient.
    /// </summary>
    private void MultiplayerClick()
    {
      ContentViewModel = new ConnectionViewModel(_windowManager, this, new Networking.Client.RiskClient("Enterprise"));
      IsEnabled = false;
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    private void QuitGameClick()
    {
      _windowManager.CloseWindow();
    }
  }
}