using Risk.Model.Enums;

namespace Risk.ViewModel.Game
{
  /// <summary>
  /// Provides contract for game board view model.
  /// </summary>
  public interface IGameBoardViewModel
  {
    /// <summary>
    /// Game dialog veiw model.
    /// </summary>
    ViewModelBase GameDialogViewModel { get; set; }

    /// <summary>
    /// Color of player.
    /// </summary>
    ArmyColor PlayerColor { get; }

    /// <summary>
    /// If game board is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Number of free units.
    /// </summary>
    int FreeArmy { get; set; }

    /// <summary>
    /// First selected planet.
    /// </summary>
    Planet Selected1 { get; }

    /// <summary>
    /// Second selected planet.
    /// </summary>
    Planet Selected2 { get; }
  }
}