using Risk.Networking.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Risk.Networking.Messages.Data;

namespace Risk.ViewModel.Multiplayer
{
  public class CreateGameDialogViewModel : ViewModelBase
  {
    private IMultiplayerMenuViewModel _multiplayerViewModel;

    private IWindowManager _windowManager;

    private RiskClient _client;

    private string _gameName;

    private int _players;

    private string _map;

    private string _error;

    public ICommand Create_Click { get; private set; }

    public ICommand Cancel_Click { get; private set; }

    public string GameName
    {
      get
      {
        return _gameName;
      }
      set
      {
        _gameName = value;
        OnPropertyChanged("GameName");
      }
    }

    public int Players
    {
      get
      {
        return _players;
      }
      set
      {
        _players = value;
        OnPropertyChanged("Players");
      }
    }

    public string Map
    {
      get
      {
        return _map;
      }
      set
      {
        _map = value;
        OnPropertyChanged("Map");
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

    public ObservableCollection<string> Maps { get; private set; }

    public ObservableCollection<int> NumberOfPlayers { get; private set; }

    public CreateGameDialogViewModel(IWindowManager windowManager, IMultiplayerMenuViewModel multiplayerViewModel, RiskClient client)
    {
      _multiplayerViewModel = multiplayerViewModel;
      _windowManager = windowManager;
      _client = client;

      Create_Click = new Command(CreateClick);
      Cancel_Click = new Command(CancelClick);

      Maps = CreateMaps();
      NumberOfPlayers = CreateNumberOfPlayers();
    }

    private async void CreateClick()
    {
      bool result = await _client.SendCreateGameRequest(new CreateGameRoomInfo(GameName, Players, Map == "Default"));
      if (result)
      {
        _windowManager.WindowViewModel = new MultiplayerRoomViewModel(_windowManager, _client);
      }
      else
      {
        Error = "Game with this name already exists!";
      }
    }

    private void CancelClick()
    {
      _multiplayerViewModel.DialogViewModel = null;
      _multiplayerViewModel.IsEnabled = true;
    }

    private ObservableCollection<string> CreateMaps()
    {
      var maps = new ObservableCollection<string>();
      maps.Add("Default");
      maps.Add("Generate");
      return maps;
    }

    private ObservableCollection<int> CreateNumberOfPlayers()
    {
      var numberOfPlayers = new ObservableCollection<int>();
      for (int i = 3; i <= 6; i++)
      {
        numberOfPlayers.Add(i);
      }
      return numberOfPlayers;
    }
  }
}