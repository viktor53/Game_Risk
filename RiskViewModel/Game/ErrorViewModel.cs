using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public class ErrorViewModel : ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVM;

    private string _errorText;

    public ICommand OK_Click { get; private set; }

    public string ErrorText
    {
      get
      {
        return _errorText;
      }
      set
      {
        _errorText = value;
        OnPropertyChanged("ErrorText");
      }
    }

    public ErrorViewModel(IGameBoardViewModel gameBoardVM, string error)
    {
      _gameBoardVM = gameBoardVM;
      ErrorText = error;

      OK_Click = new Command(OKClick);
    }

    private void OKClick()
    {
      _gameBoardVM.IsEnabled = true;
      _gameBoardVM.GameDialogViewModel = null;
    }
  }
}