using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Multiplayer
{
  public class ConnectToGameErrorDialogViewModel : ViewModelBase
  {
    private IMultiplayerMenuViewModel _multiplayerViewModel;

    public ICommand OK_Click { get; private set; }

    public ConnectToGameErrorDialogViewModel(IMultiplayerMenuViewModel multiplayerViewModel)
    {
      _multiplayerViewModel = multiplayerViewModel;

      OK_Click = new Command(OKClick);
    }

    private void OKClick()
    {
      _multiplayerViewModel.DialogViewModel = null;
      _multiplayerViewModel.IsEnabled = true;
    }
  }
}