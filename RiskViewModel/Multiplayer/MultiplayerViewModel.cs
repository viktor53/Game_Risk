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

    private SynchronizationContext ui;

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

    private GameRoomInfo _room;

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
      _client.ListenToUpdates();

      BackToMenu_Click = new Command(BackToMenuClick);
      CreateGame_Click = new Command(CreateGameClick);
      ConnectToGame_Click = new Command(ConnectToGameClick);

      Rooms = new ObservableCollection<GameRoomInfo>();
      ui = SynchronizationContext.Current;
    }

    private void BackToMenuClick()
    {
      _client.SendLougOut();
      _widnowManager.WindowViewModel = new MainMenuViewModel(_widnowManager);
    }

    private void CreateGameClick()
    {
      DialogViewModel = new CreateGameDialogViewModel(_widnowManager, this, _client);
      IsEnabled = false;
    }

    private async void ConnectToGameClick()
    {
      if (Room != null)
      {
        await _client.SendConnectToGameRequest(Room.RoomName).ContinueWith((result) =>
        {
          if (result.Result)
          {
            _widnowManager.WindowViewModel = new MultiplayerRoomViewModel(_widnowManager, _client);
          }
          else
          {
            DialogViewModel = new ConnectToGameErrorDialogViewModel(this);
            IsEnabled = false;
          }
        });
      }
    }

    private void OnUpdate(object sender, EventArgs ev)
    {
      ui.Send(x => Rooms.Clear(), null);
      foreach (var room in _client.Rooms)
      {
        Debug.WriteLine($"{room.RoomName}, {room.Connected}, {room.Capacity}", "Client");
        ui.Send(x => Rooms.Add(room), null);
      }
    }
  }
}