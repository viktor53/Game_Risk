using System.Threading;

namespace Risk.ViewModel
{
  /// <summary>
  /// Provides contract for window manager that manages main window content.
  /// </summary>
  public interface IWindowManager
  {
    /// <summary>
    /// Veiw model for window content.
    /// </summary>
    ViewModelBase WindowViewModel { get; set; }

    /// <summary>
    /// Synchronization context for updanting UI elements.
    /// </summary>
    SynchronizationContext UI { get; }

    /// <summary>
    /// Closes the window and whole application.
    /// </summary>
    void CloseWindow();
  }
}