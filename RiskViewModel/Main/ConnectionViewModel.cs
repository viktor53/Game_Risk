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

    public ICommand Connect_Click { get; private set; }

    public ICommand Cancel_Click { get; private set; }

    public ConnectionViewModel(IWindowManager mainViewModel, IMainMenuViewModel mainMenu, RiskClient client)
    {
      _windowManager = mainViewModel;
      _mainMenu = mainMenu;
      _client = client;

      Connect_Click = new Command(ConnectClick);
      Cancel_Click = new Command(CancelClick);
    }

    private void ConnectClick()
    {
      _windowManager.WindowViewModel = new MultiplayerViewModel(_windowManager);
    }

    private void CancelClick()
    {
      _mainMenu.ContentViewModel = null;
      _mainMenu.IsEnabled = true;
    }
  }
}