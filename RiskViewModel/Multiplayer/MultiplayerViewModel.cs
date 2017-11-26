using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.ViewModel.Main;
using System.Windows.Input;

namespace Risk.ViewModel.Multiplayer
{
  public class MultiplayerViewModel: ViewModelBase, IMultiplayerMenuViewModel
  {
    private IWindowManager _widnowManager;

    private ViewModelBase _dialogViewModel;

    private bool _isEnabled = true;
    
    public ICommand BackToMenu_Click { get; private set; }

    public ICommand CreateGame_Click { get; private set; }

    public ICommand ConnectToGame_Click { get; private set; }

    public ViewModelBase DialogViewModel
    {
      get
      {
        return _dialogViewModel;
      }
      set
      {
        _dialogViewModel = value;
        OnPropertyChanged("DialogViewModel");
      }
    }

    public bool IsEnabled {
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

    public ObservableCollection<FakeData> TestData { get; private set; }

    public MultiplayerViewModel(IWindowManager windowManager)
    {
      _widnowManager = windowManager;

      BackToMenu_Click = new Command(BackToMenuClick);
      CreateGame_Click = new Command(CreateGameClick);
      ConnectToGame_Click = new Command(ConnectToGameClick);

      TestData = new ObservableCollection<FakeData>();
      TestData.Add(new FakeData("neco1", "neco2"));
      TestData.Add(new FakeData("neco3", "neco4"));
      TestData.Add(new FakeData("neco5", "neco6"));
      TestData.Add(new FakeData("neco7", "neco8"));
    }
    
    private void BackToMenuClick()
    {
      _widnowManager.WindowViewModel = new MainMenuViewModel(_widnowManager);
    }

    private void CreateGameClick()
    {
      DialogViewModel = new CreateGameDialogViewModel(this);
      IsEnabled = false;
    }

    private void ConnectToGameClick()
    {

    }
  }


  public class FakeData
  {
    public string Name { get; private set; }

    public string Players { get; private set; }

    public FakeData(string name, string players)
    {
      Name = name;
      Players = players;
    }
  }
}
