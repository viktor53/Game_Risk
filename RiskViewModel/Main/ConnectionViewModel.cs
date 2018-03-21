using System.Windows.Input;
using Risk.ViewModel.Multiplayer;
using Risk.Networking.Client;

namespace Risk.ViewModel.Main
{
  /// <summary>
  /// Represents connecting to risk server.
  /// </summary>
  public class ConnectionViewModel : ViewModelBase
  {
    private IWindowManager _windowManager;

    private IMainMenuViewModel _mainMenu;

    private RiskClient _client;

    private string _name;

    private string _error;

    private bool _isConnected;

    /// <summary>
    /// Name of player.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        _name = value;
        OnPropertyChanged("Name");
      }
    }

    /// <summary>
    /// Error during connecting.
    /// </summary>
    public string Error
    {
      get
      {
        return _error;
      }
      set
      {
        _error = value;
        OnPropertyChanged("Error");
      }
    }

    /// <summary>
    /// Click on Connect button. Tries to connect to risk server and registrates player.
    /// </summary>
    public ICommand Connect_Click { get; private set; }

    /// <summary>
    /// Click on Cancel button. Cancels connecting and returns to main menu.
    /// </summary>
    public ICommand Cancel_Click { get; private set; }

    public ConnectionViewModel(IWindowManager mainViewModel, IMainMenuViewModel mainMenu, RiskClient client)
    {
      _windowManager = mainViewModel;
      _mainMenu = mainMenu;
      _client = client;
      _isConnected = false;

      Name = "Your nickname...";

      Connect_Click = new Command(ConnectClick);
      Cancel_Click = new Command(CancelClick);
    }

    /// <summary>
    /// Tries to connect to risk server and registrates player.
    /// </summary>
    private async void ConnectClick()
    {
      bool succes = _isConnected ? true : await _client.ConnectAsync();
      if (succes)
      {
        _isConnected = true;

        succes = await _client.SendRegistrationRequestAsync(_name);
        if (succes)
        {
          _windowManager.WindowViewModel = new MultiplayerViewModel(_windowManager, _client);
        }
        else
        {
          Error = "The name already exists!";
        }
      }
      else
      {
        Error = "Can not connect to server!";
      }
    }

    /// <summary>
    /// Cancels connecting and returns to main menu.
    /// </summary>
    private async void CancelClick()
    {
      await _client.SendLougOutRequestAsync();
      _mainMenu.ContentViewModel = null;
      _mainMenu.IsEnabled = true;
    }
  }
}