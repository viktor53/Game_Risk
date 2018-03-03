using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Risk.ViewModel.Game;

namespace Risk.ViewModel.Multiplayer
{
  public class MultiplayerRoomViewModel: ViewModelBase
  {
    private IWindowManager _windowManager;

    private bool _isEnableb = true;

    private string _text = "";

    public ICommand Ready_Click { get; private set; }

    public ICommand Cancel_Click { get; private set; }

    public ObservableCollection<string> Players { get; private set; }

    public bool IsEnabled {
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

    public MultiplayerRoomViewModel(IWindowManager windowManager)
    {
      _windowManager = windowManager;

      Ready_Click = new Command(ReadyClick);
      Cancel_Click = new Command(CancelClick);

      Players = new ObservableCollection<string>();

      Players.Add("PlayerOne");
      Players.Add("PLayerTwo");
    }

    private void ReadyClick()
    {
      Text = "Wainting on players...";

      IsEnabled = false;

      _windowManager.WindowViewModel = new GameBoardViewModel(_windowManager);
    }

    private  void CancelClick()
    {
      _windowManager.WindowViewModel = new MultiplayerViewModel(_windowManager);
    }

  }
}
