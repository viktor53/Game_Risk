using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;
using Risk.Model.GameCore;

namespace Risk.Networking.Server
{
  /// <summary>
  /// Provides contract for players and client managers, that represent connection with clients.
  /// </summary>
  public interface IClientManager : IPlayer
  {
    /// <summary>
    /// Name of connected player.
    /// </summary>
    string PlayerName { get; }

    /// <summary>
    /// Game room where player is connected.
    /// </summary>
    IGameRoom GameRoom { get; set; }

    /// <summary>
    /// OnReady is raised whenever connected player to game room is ready to play.
    /// </summary>
    event EventHandler OnReady;

    /// <summary>
    /// OnLeave is raised whenever connected player to game room leaved.
    /// </summary>
    event EventHandler OnLeave;

    /// <summary>
    /// Asynchronously sends notification about new player is connected to game room.
    /// </summary>
    /// <param name="name">name of the player</param>
    /// <returns>async task</returns>
    Task SendNewPlayerConnected(string name);

    /// <summary>
    /// Asynchronously sends notification about player left tha game room.
    /// </summary>
    /// <param name="name">name of the player</param>
    /// <returns>async task</returns>
    Task SendPlayerLeave(string name);

    /// <summary>
    /// Asynchronously sends inforamtion about connected players in the game room.
    /// </summary>
    /// <param name="players">connected players</param>
    /// <returns>async task</returns>
    Task SendConnectedPlayers(IList<string> players);

    /// <summary>
    /// Asynchronously listens to ready tag or leaving notification from client.
    /// </summary>
    /// <returns>async task</returns>
    Task ListenToReadyTag();

    /// <summary>
    /// Asynchronously listens to client's commands before connecting to game room.
    /// </summary>
    /// <returns>async task</returns>
    Task SartListening();

    /// <summary>
    /// Asynchronously sends update of game list.
    /// </summary>
    /// <param name="roomsInfo">updated game room information list</param>
    /// <returns>async task</returns>
    Task SendUpdateGameList(IList<GameRoomInfo> roomsInfo);
  }
}