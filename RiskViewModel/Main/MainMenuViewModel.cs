using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace Risk.ViewModel.Main
{
  public class MainMenuViewModel : ViewModelBase, IMainMenuViewModel
  {
    private ViewModelBase _contentViewModel;

    private IWindowManager _windowManager;

    private bool _isEnabled = true;

    public ICommand Multiplayer_Click { get; private set; }

    public ICommand Singleplayer_Click { get; private set; }

    public ICommand QuitGame_Click { get; private set; }

    public ViewModelBase ContentViewModel
    {
      get
      {
        return _contentViewModel;
      }

      set
      {
        _contentViewModel = value;
        this.OnPropertyChanged("ContentViewModel");
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

    public MainMenuViewModel(IWindowManager windowManager)
    {
      Multiplayer_Click = new Command(MultiplayerClick);

      Singleplayer_Click = new Command(SingleplayerClick);

      QuitGame_Click = new Command(QuitGameClick);

      _windowManager = windowManager;
    }

    private void SingleplayerClick()
    {
      ContentViewModel = new IntroSinglePlayerViewModel();
    }

    private void MultiplayerClick()
    {
      ContentViewModel = new ConnectionViewModel(_windowManager, this, new Networking.Client.RiskClient());
      IsEnabled = false;
    }

    private void QuitGameClick()
    {
      _windowManager.CloseWindow();
    }
  }
}