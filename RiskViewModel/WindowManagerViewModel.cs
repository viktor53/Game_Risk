using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.ViewModel.Main;
using System.Windows;
using System.Threading;

namespace Risk.ViewModel
{
  public class WindowManagerViewModel : ViewModelBase, IWindowManager
  {
    private ViewModelBase _windowViewModel;

    private SynchronizationContext _ui;

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

    public SynchronizationContext UI => _ui;

    public WindowManagerViewModel()
    {
      WindowViewModel = new MainMenuViewModel(this);
      _ui = SynchronizationContext.Current;
    }

    public void CloseWindow()
    {
      Application.Current.MainWindow.Close();
    }
  }
}