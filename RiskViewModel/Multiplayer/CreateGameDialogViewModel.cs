using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Risk.Networking.Messages.Data;
using Risk.Networking.Client;

namespace Risk.ViewModel.Multiplayer
{
  /// <summary>
  /// Represents creating new game room.
  /// </summary>
  public class CreateGameDialogViewModel : ViewModelBase
  {
    private IMultiplayerMenuViewModel _multiplayerViewModel;

    private IWindowManager _windowManager;

    private RiskClient _client;

    private string _gameName;

    private int _players;

    private string _map;

    private string _error;

    /// <summary>
    /// Click on Create button. Tries create new game room.
    /// </summary>
    public ICommand Create_Click { get; private set; }

    /// <summary>
    /// Click on Cancel button. Leaves creating new game room.
    /// </summary>
    public ICommand Cancel_Click { get; private set; }

    /// <summary>
    /// Name of new game room.
    /// </summary>
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

    /// <summary>
    /// Maximum capacity of players
    /// </summary>
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

    /// <summary>
    /// Type of map. Default/Classic map or random generated map.
    /// </summary>
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

    /// <summary>
    /// Eroor during creating new game room.
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
    /// List of map options.
    /// </summary>
    public ObservableCollection<string> Maps { get; private set; }

    /// <summary>
    /// Number of players options list.
    /// </summary>
    public ObservableCollection<int> NumberOfPlayers { get; private set; }

    public CreateGameDialogViewModel(IWindowManager windowManager, IMultiplayerMenuViewModel multiplayerViewModel, RiskClient client)
    {
      _multiplayerViewModel = multiplayerViewModel;
      _windowManager = windowManager;
      _client = client;
      _client.OnConfirmation += OnConfirmation;

      Create_Click = new Command(CreateClick);
      Cancel_Click = new Command(CancelClick);

      Maps = CreateMaps();
      NumberOfPlayers = CreateNumberOfPlayers();
    }

    /// <summary>
    /// Tries create new game room.
    /// </summary>
    private async void CreateClick()
    {
      await _client.SendCreateGameRequestAsync(new CreateGameRoomInfo(GameName, Players, Map == "Default"));
    }

    /// <summary>
    /// Leaves creating new game room.
    /// </summary>
    private void CancelClick()
    {
      _multiplayerViewModel.DialogViewModel = null;
      _multiplayerViewModel.IsEnabled = true;
    }

    /// <summary>
    /// Method that is called when OnConfiramtion event is raised.
    /// If confirmation is positive changes WindowViewModel on MultiplayerRoomViewModel otherwise writes error.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
    private void OnConfirmation(object sender, EventArgs ev)
    {
      if (((ConfirmationEventArgs)ev).Data)
      {
        _client.OnConfirmation -= OnConfirmation;
        _windowManager.WindowViewModel = new MultiplayerRoomViewModel(_windowManager, _client);
      }
      else
      {
        Error = "Game with this name already exists!";
      }
    }

    /// <summary>
    /// Creates list of map options.
    /// </summary>
    /// <returns></returns>
    private ObservableCollection<string> CreateMaps()
    {
      var maps = new ObservableCollection<string>();
      maps.Add("Default");
      maps.Add("Generate");
      return maps;
    }

    /// <summary>
    /// Creates number of players options list.
    /// </summary>
    /// <returns></returns>
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