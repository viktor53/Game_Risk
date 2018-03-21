namespace Risk.ViewModel.Main
{
  /// <summary>
  /// Provides contract for main menu view model.
  /// </summary>
  public interface IMainMenuViewModel
  {
    /// <summary>
    /// View model for content of main menu.
    /// </summary>
    ViewModelBase ContentViewModel { get; set; }

    /// <summary>
    /// If main menu is enabled.
    /// </summary>
    bool IsEnabled { get; set; }
  }
}