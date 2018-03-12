using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Risk.ViewModel.Game;
using Risk.Networking.Client;
using System.Threading;

namespace Risk.ViewModel.Multiplayer
{
  public class MultiplayerRoomViewModel : ViewModelBase
  {
    private IWindowManager _windowManager;

    private RiskClient _client;

    private bool _isEnableb;

    private string _text = "";

    public ICommand Ready_Click { get; private set; }

    public ICommand Cancel_Click { get; private set; }

    public ObservableCollection<string> Players { get; private set; }

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

    public MultiplayerRoomViewModel(IWindowManager windowManager, RiskClient client)
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

    private void ReadyClick()
    {
      Text = "Wainting on players...";

      IsEnabled = false;

      _client.SendReadyTag();
    }

    private void CancelClick()
    {
      _client.SendLeaveGame();

      _client.OnUpdate -= OnUpdate;
      _client.OnInicialization -= OnInicialization;

      _windowManager.WindowViewModel = new MultiplayerViewModel(_windowManager, _client);
    }

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

    private void OnInicialization(object sender, EventArgs e)
    {
      _client.OnUpdate -= OnUpdate;
      _client.OnInicialization -= OnInicialization;

      _windowManager.WindowViewModel = new GameBoardViewModel(_windowManager, _client, ((InicializationEventArgs)e).Data);
    }
  }
}