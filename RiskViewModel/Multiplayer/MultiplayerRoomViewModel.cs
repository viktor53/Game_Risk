using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Risk.ViewModel.Game;
using Risk.Networking.Client;

namespace Risk.ViewModel.Multiplayer
{
  /// <summary>
  /// Represents multiplayer room where players wait on other players.
  /// </summary>
  public class MultiplayerRoomViewModel : ViewModelBase
  {
    private IWindowManager _windowManager;

    private IClient _client;

    private bool _isEnableb;

    private string _text = "";

    /// <summary>
    /// Click on Ready button. Notifies that player is ready.
    /// </summary>
    public ICommand Ready_Click { get; private set; }

    /// <summary>
    /// Click on Cancel button. Leaves game room.
    /// </summary>
    public ICommand Cancel_Click { get; private set; }

    /// <summary>
    /// List of player names.
    /// </summary>
    public ObservableCollection<string> Players { get; private set; }

    /// <summary>
    /// If it is enabled.
    /// </summary>
    public bool IsEnabled
    {
      get
      {
        return _isEnableb;
      }
      set
      {
        _isEnableb = value;
        OnPropertyChanged("IsEnabled");
      }
    }

    /// <summary>
    /// Text with information about process.
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
    /// Initializes MultiplayerRoomViewModel.
    /// </summary>
    /// <param name="windowManager">window manager</param>
    /// <param name="client">risk client connected to server and listening to updates</param>
    public MultiplayerRoomViewModel(IWindowManager windowManager, IClient client)
    {
      _windowManager = windowManager;
      _client = client;
      _client.OnUpdate += OnUpdate;
      _client.OnInicialization += OnInicialization;

      Ready_Click = new Command(ReadyClick);
      Cancel_Click = new Command(CancelClick);

      Players = new ObservableCollection<string>();

      IsEnabled = false;
    }

    /// <summary>
    /// Notifies that player is ready.
    /// </summary>
    private async void ReadyClick()
    {
      Text = "Wainting on players...";

      IsEnabled = false;

      await _client.SendReadyTagRequestAsync();
    }

    /// <summary>
    /// Leaves game room.
    /// </summary>
    private async void CancelClick()
    {
      await _client.SendLeaveGameRequestAsync();

      _client.OnUpdate -= OnUpdate;
      _client.OnInicialization -= OnInicialization;

      _windowManager.WindowViewModel = new MultiplayerViewModel(_windowManager, _client);
    }

    /// <summary>
    /// Method is called when OnUpdate event is raised. Updates list of connected players.
    /// </summary>
    /// <param name="sender">client that raised the event</param>
    /// <param name="e">EventArgs is not used</param>
    private void OnUpdate(object sender, EventArgs e)
    {
      IsEnabled = true;

      Text = "";

      _windowManager.UI.Send(x => Players.Clear(), null);
      foreach (var player in _client.Players)
      {
        _windowManager.UI.Send(x => Players.Add(player), null);
      }
    }

    /// <summary>
    /// Method is called whe OnInicializtation event is raised. Inicializes GameBoardViewModel.
    /// </summary>
    /// <param name="sender">client that raised the event</param>
    /// <param name="e">InicializationEventArgs</param>
    private void OnInicialization(object sender, EventArgs e)
    {
      _client.OnUpdate -= OnUpdate;
      _client.OnInicialization -= OnInicialization;

      _windowManager.WindowViewModel = new GameBoardViewModel(_windowManager, _client, ((InicializationEventArgs)e).Data);
    }
  }
}