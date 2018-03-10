using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.ViewModel.Main;
using System.Windows.Input;
using Risk.Networking.Client;
using Risk.Networking.Messages.Data;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Risk.ViewModel.Multiplayer
{
  public class MultiplayerViewModel : ViewModelBase, IMultiplayerMenuViewModel
  {
    private IWindowManager _widnowManager;

    private ViewModelBase _dialogViewModel;

    private RiskClient _client;

    private bool _isEnabled = true;

    private SynchronizationContext _ui;

    private GameRoomInfo _room;

    public ICommand BackToMenu_Click { get; private set; }

    public ICommand CreateGame_Click { get; private set; }

    public ICommand ConnectToGame_Click { get; private set; }

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

    public ObservableCollection<GameRoomInfo> Rooms { get; private set; }

    public MultiplayerViewModel(IWindowManager windowManager, RiskClient client)
    {
      _widnowManager = windowManager;
      _client = client;
      _client.OnUpdate += OnUpdate;
      _client.OnConfirmation += OnConfirmation;
      _client.StartListenToUpdates();

      BackToMenu_Click = new Command(BackToMenuClick);
      CreateGame_Click = new Command(CreateGameClick);
      ConnectToGame_Click = new Command(ConnectToGameClick);

      Rooms = new ObservableCollection<GameRoomInfo>();
      _ui = SynchronizationContext.Current;
    }

    private void BackToMenuClick()
    {
      _client.SendLougOut();
      _widnowManager.WindowViewModel = new MainMenuViewModel(_widnowManager);
    }

    private void CreateGameClick()
    {
      _client.OnConfirmation -= OnConfirmation;
      _client.OnUpdate -= OnUpdate;
      DialogViewModel = new CreateGameDialogViewModel(_widnowManager, this, _client, _ui);
      IsEnabled = false;
    }

    private async void ConnectToGameClick()
    {
      if (Room != null)
      {
        await _client.SendConnectToGameRequest(Room.RoomName);
      }
    }

    private void OnUpdate(object sender, EventArgs ev)
    {
      _ui.Send(x => Rooms.Clear(), null);
      foreach (var room in _client.Rooms)
      {
        Debug.WriteLine($"{room.RoomName}, {room.Connected}, {room.Capacity}", "Client");
        _ui.Send(x => Rooms.Add(room), null);
      }
    }

    private void OnConfirmation(object sender, EventArgs ev)
    {
      if (((ConfirmationEventArgs)ev).Data)
      {
        _client.OnConfirmation -= OnConfirmation;
        _client.OnUpdate -= OnUpdate;
        _widnowManager.WindowViewModel = new MultiplayerRoomViewModel(_widnowManager, _client, _ui);
      }
      else
      {
        DialogViewModel = new ConnectToGameErrorDialogViewModel(this);
        IsEnabled = false;
      }
    }
  }
}