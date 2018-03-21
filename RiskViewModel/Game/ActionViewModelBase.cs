using System.Windows.Input;
using Risk.Networking.Client;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Base class for action view model. (attack, draft, fortify...)
  /// </summary>
  public abstract class ActionViewModelBase : ViewModelBase
  {
    private IGameBoardViewModel _gameBoardVM;

    private RiskClient _client;

    private int _army;

    private string _errorText;

    /// <summary>
    /// Click on Cancel button. Returns to game board.
    /// </summary>
    public ICommand Cancel_Click { get; private set; }

    /// <summary>
    /// Click on Action button. Makes the action.
    /// </summary>
    public ICommand Action_Click { get; protected set; }

    /// <summary>
    /// Game board veiw model.
    /// </summary>
    protected IGameBoardViewModel GameBoardVM => _gameBoardVM;

    /// <summary>
    /// Player manager that is allowed to make action.
    /// </summary>
    protected RiskClient Client => _client;

    /// <summary>
    /// Number of units.
    /// </summary>
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

    /// <summary>
    /// Error message if error occurs.
    /// </summary>
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

    /// <summary>
    /// Initilaizes base of action view model.
    /// </summary>
    /// <param name="gameBoardVM">game board view model</param>
    /// <param name="client">player manager that is allowed to make action</param>
    public ActionViewModelBase(IGameBoardViewModel gameBoardVM, RiskClient client)
    {
      _gameBoardVM = gameBoardVM;
      _client = client;

      Army = 1;
      ErrorText = "";

      Cancel_Click = new Command(CancelClick);
    }

    /// <summary>
    /// Returns to game board.
    /// </summary>
    private void CancelClick()
    {
      _gameBoardVM.GameDialogViewModel = null;
      _gameBoardVM.IsEnabled = true;
    }
  }
}