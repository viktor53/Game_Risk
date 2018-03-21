using System.ComponentModel;

namespace Risk.ViewModel
{
  /// <summary>
  /// Base class for each view model.
  /// </summary>
  public abstract class ViewModelBase : INotifyPropertyChanged
  {
    /// <summary>
    /// PropertyChanged is raised whenever some propery is changed.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises event property changed.
    /// </summary>
    /// <param name="propertyName">name of changed property</param>
    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}