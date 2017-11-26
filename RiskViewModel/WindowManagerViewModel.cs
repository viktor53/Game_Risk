using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.ViewModel.Main;
using System.Windows;

namespace Risk.ViewModel
{
  public class WindowManagerViewModel: ViewModelBase, IWindowManager
  {
    private ViewModelBase _windowViewModel;

    public ViewModelBase WindowViewModel
    {
      get
      {
        return _windowViewModel;
      }

      set
      {
        _windowViewModel = value;
        OnPropertyChanged("WindowViewModel");
      }
    }

    public WindowManagerViewModel()
    {
      WindowViewModel = new MainMenuViewModel(this);
    }

    public void CloseWindow()
    {
      Application.Current.MainWindow.Close();
    }
  }
}
