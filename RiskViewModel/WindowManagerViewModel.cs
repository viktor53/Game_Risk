using System.Windows;
using System.Threading;
using Risk.ViewModel.Main;

namespace Risk.ViewModel
{
  /// <summary>
  /// Represents main window manager.
  /// </summary>
  public class WindowManagerViewModel : ViewModelBase, IWindowManager
  {
    private ViewModelBase _windowViewModel;

    private SynchronizationContext _ui;

    /// <summary>
    /// Veiw model for window content.
    /// </summary>
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

    /// <summary>
    /// Synchronization context for updanting UI elements.
    /// </summary>
    public SynchronizationContext UI => _ui;

    /// <summary>
    /// Creates window manager view model and loads main menu view model.
    /// </summary>
    public WindowManagerViewModel()
    {
      WindowViewModel = new MainMenuViewModel(this);
      _ui = SynchronizationContext.Current;
    }

    /// <summary>
    /// Closes the window and whole application.
    /// </summary>
    public void CloseWindow()
    {
      Application.Current.MainWindow.Close();
    }
  }
}