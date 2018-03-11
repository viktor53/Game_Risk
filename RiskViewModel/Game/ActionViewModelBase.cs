using Risk.Networking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Game
{
  public abstract class ActionViewModelBase : ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVM;

    private RiskClient _client;

    private int _army;

    private string _errorText;

    public ICommand Cancel_Click { get; private set; }

    public ICommand Action_Click { get; protected set; }

    protected IGameBoardViewModel GameBoardVM => _gameBoardVM;

    protected RiskClient Client => _client;

    public int Army
    {
      get
      {
        return _army;
      }
      set
      {
        _army = value;
        OnPropertyChanged("Army");
      }
    }

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

    public ActionViewModelBase(IGameBoardViewModel gameBoardVM, RiskClient client)
    {
      _gameBoardVM = gameBoardVM;
      _client = client;

      Army = 1;
      ErrorText = "";

      Cancel_Click = new Command(CancelClick);
    }

    private void CancelClick()
    {
      _gameBoardVM.GameDialogViewModel = null;
      _gameBoardVM.IsEnabled = true;
    }
  }
}