using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Risk.ViewModel.Multiplayer
{
  public class CreateGameDialogViewModel: ViewModelBase
  {
    private IMultiplayerMenuViewModel _multiplayerViewModel;

    private IWindowManager _windowManager;

    public ICommand Create_Click { get; private set; }

    public ICommand Cancel_Click { get; private set; }

    public ObservableCollection<string> Maps { get; private set; }

    public ObservableCollection<int> NumberOfPlayers { get; private set; }

    public CreateGameDialogViewModel(IWindowManager windowManager, IMultiplayerMenuViewModel multiplayerViewModel)
    {
      _multiplayerViewModel = multiplayerViewModel;
      _windowManager = windowManager;

      Create_Click = new Command(CreateClick);
      Cancel_Click = new Command(CancelClick);

      Maps = CreateMaps();
      NumberOfPlayers = CreateNumberOfPlayers();
    }

    private void CreateClick()
    {
      _windowManager.WindowViewModel = new MultiplayerRoomViewModel(_windowManager);
    }

    private void CancelClick()
    {
      _multiplayerViewModel.DialogViewModel = null;
      _multiplayerViewModel.IsEnabled = true;
    }

    private ObservableCollection<string> CreateMaps()
    {
      var maps = new ObservableCollection<string>();
      maps.Add("Default");
      maps.Add("Generate");
      return maps;
    }

    private ObservableCollection<int> CreateNumberOfPlayers()
    {
      var numberOfPlayers = new ObservableCollection<int>();
      for(int i = 1; i <= 8; i++)
      {
        numberOfPlayers.Add(i);
      }
      return numberOfPlayers;
    }
  }
}
