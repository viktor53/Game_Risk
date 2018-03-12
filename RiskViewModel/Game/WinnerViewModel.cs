using Risk.Networking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Risk.ViewModel.Multiplayer;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class WinnerViewModel : ViewModelBase
  {
    private string _text;

    private IWindowManager _windowManager;

    private ViewModelBase _viewModel;

    public ICommand OK_Click { get; private set; }

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

    public WinnerViewModel(IWindowManager windowManager, ViewModelBase menu, bool isWinner)
    {
      _viewModel = menu;
      _windowManager = windowManager;

      OK_Click = new Command(OKClick);

      if (isWinner)
      {
        Text = "You Win!";
      }
      else
      {
        Text = "You lose!";
      }
    }

    private void OKClick()
    {
      _windowManager.WindowViewModel = _viewModel;
    }
  }
}