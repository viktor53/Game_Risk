using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Server
{
  /// <summary>
  /// Provides contract for game room on the server side.
  /// </summary>
  public interface IGameRoom
  {
    /// <summary>
    /// Name of game room.
    /// </summary>
    string RoomName { get; }

    /// <summary>
    /// Maximum capacity of game room.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Number of connected players.
    /// </summary>
    int Connected { get; }

    /// <summary>
    /// OnStart is raised whenever game started.
    /// </summary>
    event EventHandler OnStart;

    /// <summary>
    /// Adds new player into game room.
    /// </summary>
    /// <param name="player">new player</param>
    /// <returns>true if player is added otherwise false</returns>
    bool AddPlayer(IClientManager player);

    /// <summary>
    /// Removes the player from the game room.
    /// </summary>
    /// <param name="playerName">name of player that will be removed</param>
    void RemovePlayer(string playerName);

    /// <summary>
    /// Gets all connected players.
    /// </summary>
    /// <returns>connected players</returns>
    IList<string> GetPlayers();

    /// <summary>
    /// If game room is full or not.
    /// </summary>
    /// <returns>if game room is full or not</returns>
    bool IsFull();

    /// <summary>
    /// Gets game board informations.
    /// </summary>
    /// <param name="gamePlan">game plan information</param>
    /// <returns>game board information</returns>
    GameBoardInfo GetBoardInfo(GamePlanInfo gamePlan);
  }
}