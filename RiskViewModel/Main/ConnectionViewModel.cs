using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Risk.ViewModel.Multiplayer;
using Risk.Networking.Client;

namespace Risk.ViewModel.Main
{
  public class ConnectionViewModel : ViewModelBase
  {
    private IWindowManager _windowManager;

    private IMainMenuViewModel _mainMenu;

    private RiskClient _client;

    private string _name;

    private string _error;

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

    public ICommand Connect_Click { get; private set; }

    public ICommand Cancel_Click { get; private set; }

    public ConnectionViewModel(IWindowManager mainViewModel, IMainMenuViewModel mainMenu, RiskClient client)
    {
      _windowManager = mainViewModel;
      _mainMenu = mainMenu;
      _client = client;

      Name = "Your nickname...";

      Connect_Click = new Command(ConnectClick);
      Cancel_Click = new Command(CancelClick);
    }

    private async void ConnectClick()
    {
      //TODO change positon of Connecting
      _client.ConnectAsync();
      bool succes = await _client.SendRegistrationRequestAsync(_name);
      if (succes)
      {
        _windowManager.WindowViewModel = new MultiplayerViewModel(_windowManager, _client);
      }
      else
      {
        Error = "The name already exists!";
      }
    }

    private void CancelClick()
    {
      _mainMenu.ContentViewModel = null;
      _mainMenu.IsEnabled = true;
    }
  }
}