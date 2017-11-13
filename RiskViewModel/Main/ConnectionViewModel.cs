using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Main
{
  public class ConnectionViewModel: ViewModelBase
  {
    private MainMenuViewModel _mainViewModel;

    public ICommand Connect_Click { get; private set; }

    public ConnectionViewModel(MainMenuViewModel mainViewModel)
    {
      _mainViewModel = mainViewModel;
      Connect_Click = new ParameterCommand(ConnectClick);
    }

    private void  ConnectClick(object parametr)
    {
      _mainViewModel.MultiplayerViewModel = new IntroSinglePlayerViewModel();
    }
  }
}
