using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Client
{
  /// <summary>
  /// Provides contract for client.
  /// </summary>
  public interface IClient : IPlayer
  {
    /// <summary>
    /// OnConfirmation is raised whenever confirmation is received from server.
    /// </summary>
    event EventHandler OnConfirmation;

    /// <summary>
    /// OnInicialization is raised whenever game is inicializated.
    /// </summary>
    event EventHandler OnInicialization;

    /// <summary>
    /// List of opened game rooms.
    /// </summary>
    IList<GameRoomInfo> Rooms { get; }

    /// <summary>
    /// List of connected players to game room.
    /// </summary>
    IList<string> Players { get; }

    /// <summary>
    /// Asynchronously connects to server.
    /// </summary>
    /// <returns>true if connecting succeeded, otherwise false</returns>
    Task<bool> ConnectAsync();

    /// <summary>
    /// Asynchronously sends registration request.
    /// </summary>
    /// <param name="name">name to be registered</param>
    /// <returns>true if sending succeeded, otherwise false</returns>
    Task<bool> SendRegistrationRequestAsync(string name);

    /// <summary>
    /// Asynchronously listens to updates of game list and player list.
    /// </summary>
    /// <returns>true if listening succeeded, otherwise false</returns>
    Task<bool> ListenToUpdatesAsync();

    /// <summary>
    /// Asynchronously sends connect to the game request.
    /// </summary>
    /// <param name="gameName">name of the game</param>
    /// <returns>true if sending succeeded, otherwise false</returns>
    Task<bool> SendConnectToGameRequestAsync(string gameName);

    /// <summary>
    /// Asynchronously sends create game request.
    /// </summary>
    /// <param name="roomInfo">information about creating game</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendCreateGameRequestAsync(CreateGameRoomInfo roomInfo);

    /// <summary>
    /// Asynchronously sends leave game request.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendLeaveGameRequestAsync();

    /// <summary>
    /// Asynchronously sends ready tag request.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendReadyTagRequestAsync();

    /// <summary>
    /// Asynchronously sends logout request.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendLougOutRequestAsync();
  }
}