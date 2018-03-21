using System;
using System.Collections.ObjectModel;
using Risk.ViewModel.Main;
using System.Windows.Input;
using Risk.Networking.Client;
using Risk.Networking.Messages.Data;

namespace Risk.ViewModel.Multiplayer
{
  /// <summary>
  /// Represents multiplayer menu with opened game rooms. Implements IMultiplayerMenuViewModel.
  /// </summary>
  public class MultiplayerViewModel : ViewModelBase, IMultiplayerMenuViewModel
  {
    private IWindowManager _widnowManager;

    private ViewModelBase _dialogViewModel;

    private IClient _client;

    private bool _isEnabled = true;

    private GameRoomInfo _room;

    /// <summary>
    /// Click on Back to Menu button. Logs out player and returns to main menu.
    /// </summary>
    public ICommand BackToMenu_Click { get; private set; }

    /// <summary>
    /// Click on Create Game button. Changes DialogViewModel on CreateGameDialogViewModel.
    /// </summary>
    public ICommand CreateGame_Click { get; private set; }

    /// <summary>
    /// Click on Connect to Game button. Tries to connect to selected game room.
    /// </summary>
    public ICommand ConnectToGame_Click { get; private set; }

    /// <summary>
    /// Dialog view model.
    /// </summary>
    public ViewModelBase DialogViewModel
    {
      get
      {
        return _dialogViewModel;
      }
      set
      {
        _dialogViewModel = value;
        OnPropertyChanged("DialogViewModel");
        _client.OnConfirmation += OnConfirmation;
        _client.OnUpdate += OnUpdate;
      }
    }

    /// <summary>
    /// If multiplayer menu is enabled.
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
    /// Selected game room.
    /// </summary>
    public GameRoomInfo Room
    {
      get
      {
        return _room;
      }
      set
      {
        _room = value;
        OnPropertyChanged("Room");
      }
    }

    /// <summary>
    /// List of opened game rooms.
    /// </summary>
    public ObservableCollection<GameRoomInfo> Rooms { get; private set; }

    /// <summary>
    /// Initializes MultiplayerViewModel and starts listen to updates.
    /// </summary>
    /// <param name="windowManager">window manager</param>
    /// <param name="client">risk client connected to server</param>
    public MultiplayerViewModel(IWindowManager windowManager, IClient client)
    {
      _widnowManager = windowManager;
      _client = client;
      _client.OnUpdate += OnUpdate;
      _client.OnConfirmation += OnConfirmation;
      _client.ListenToUpdatesAsync();

      BackToMenu_Click = new Command(BackToMenuClick);
      CreateGame_Click = new Command(CreateGameClick);
      ConnectToGame_Click = new Command(ConnectToGameClick);

      Rooms = new ObservableCollection<GameRoomInfo>();
    }

    /// <summary>
    /// Logs out player and returns to main menu.
    /// </summary>
    private async void BackToMenuClick()
    {
      await _client.SendLougOutRequestAsync();
      _widnowManager.WindowViewModel = new MainMenuViewModel(_widnowManager);
    }

    /// <summary>
    /// Changes DialogViewModel on CreateGameDialogViewModel.
    /// </summary>
    private void CreateGameClick()
    {
      _client.OnConfirmation -= OnConfirmation;
      _client.OnUpdate -= OnUpdate;
      DialogViewModel = new CreateGameDialogViewModel(_widnowManager, this, _client);
      IsEnabled = false;
    }

    /// <summary>
    /// Tries to connect to selected game room.
    /// </summary>
    private async void ConnectToGameClick()
    {
      if (Room != null)
      {
        await _client.SendConnectToGameRequestAsync(Room.RoomName);
      }
    }

    /// <summary>
    /// Method that is called when OnUpdate event is raised. Updates list of game rooms.
    /// </summary>
    /// <param name="sender">who raised the event OnUpdate</param>
    /// <param name="ev">EventArgs is not used</param>
    private void OnUpdate(object sender, EventArgs ev)
    {
      _widnowManager.UI.Send(x => Rooms.Clear(), null);
      foreach (var room in _client.Rooms)
      {
        _widnowManager.UI.Send(x => Rooms.Add(room), null);
      }
    }

    /// <summary>
    /// Method that is called when OnConfirmation event is raised.
    /// If confirmation is positive, changes WindowViewModel on MultiplayerRoomViewModel,
    /// otherwise changes DialogViewModel on ConnectToGameErrorDialogViewModel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
    private void OnConfirmation(object sender, EventArgs ev)
    {
      if (((ConfirmationEventArgs)ev).Data)
      {
        _client.OnConfirmation -= OnConfirmation;
        _client.OnUpdate -= OnUpdate;
        _widnowManager.WindowViewModel = new MultiplayerRoomViewModel(_widnowManager, _client);
      }
      else
      {
        DialogViewModel = new ConnectToGameErrorDialogViewModel(this);
        IsEnabled = false;
      }
    }
  }
}