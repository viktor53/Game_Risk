using System.Collections.ObjectModel;
using Risk.Networking.Messages.Data;

namespace Risk.ViewModel.Multiplayer
{
  /// <summary>
  /// Provides contract for multiplayer menu with opened game rooms.
  /// </summary>
  public interface IMultiplayerMenuViewModel
  {
    /// <summary>
    /// If multiplayer menu is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Dialog view model.
    /// </summary>
    ViewModelBase DialogViewModel { get; set; }

    /// <summary>
    /// Selected game room.
    /// </summary>
    GameRoomInfo Room { get; set; }

    /// <summary>
    /// List of opened game rooms.
    /// </summary>
    ObservableCollection<GameRoomInfo> Rooms { get; }
  }
}