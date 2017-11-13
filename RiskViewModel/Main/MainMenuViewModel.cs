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
  public class MainMenuViewModel: ViewModelBase
  {
    private ViewModelBase _currentViewModel;
    private ViewModelBase _multiplayerViewModel;
    private ViewModelBase _singleplayerViewModel;

    public ICommand Multiplayer_Click { get; private set; }

    public ICommand Singleplayer_Click { get; private set; }

    public ICommand QuitGame_Click { get; private set; }

    public ViewModelBase CurrentViewModel
    {
      get
      {
        return _currentViewModel;
      }

      set
      {
        _currentViewModel = value;
        this.OnPropertyChanged("CurrentViewModel");
      }
    }

    public bool VisibilityMultiplayerView => _multiplayerViewModel != null;

    public ViewModelBase MultiplayerViewModel
    {
      get
      {
        return _multiplayerViewModel;
      }
      set
      {
        _multiplayerViewModel = value;
        this.OnPropertyChanged("MultiplayerViewModel");
        this.OnPropertyChanged("VisibilityMultiplayerView");
      }
    }

    public bool VisibilitySingleplayerView => _singleplayerViewModel != null;

    public ViewModelBase SingleplayerViewModel
    {
      get
      {
        return _singleplayerViewModel;
      }
      set
      {
        _singleplayerViewModel = value;
        this.OnPropertyChanged("SingleplayerViewModel");
        this.OnPropertyChanged("VisibilitySingleplayerView");
      }
    }


    public MainMenuViewModel()
    {
      Multiplayer_Click = new ParameterCommand(MultiplayerClick);

      Singleplayer_Click = new ParameterCommand(SingleplayerClick);

      QuitGame_Click = new ParameterCommand(QuitGameClick);
    }

    private void SingleplayerClick(object parameter)
    {
      CurrentViewModel = new IntroSinglePlayerViewModel();
    }

    private void MultiplayerClick(object parameter)
    {
      CurrentViewModel = new ConnectionViewModel(this);
    }

    private void QuitGameClick(object parameter)
    {
      Window w = parameter as Window;
      if(w != null)
      {
        w.Close();
      }
    }
  }
}
