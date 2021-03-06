﻿using System.Threading.Tasks;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GamePlan;

namespace Risk.Model.GameCore
{
  /// <summary>
  /// Interface providing contract for player. Needed to implement for playing game.
  /// </summary>
  public interface IPlayer
  {
    /// <summary>
    /// Color of player.
    /// </summary>
    ArmyColor PlayerColor { get; set; }

    /// <summary>
    /// Free units on start game or in draft phase of game.
    /// </summary>
    int FreeUnit { get; set; }

    /// <summary>
    /// Adds new card received from game.
    /// </summary>
    /// <param name="card">risk card</param>
    void AddCard(RiskCard card);

    /// <summary>
    /// Removes card that was exchanged.
    /// </summary>
    /// <param name="card">exchange card</param>
    void RemoveCard(RiskCard card);

    /// <summary>
    /// Asynchronously starts player. Useful for setup player or getting game plan info.
    /// </summary>
    /// <param name="game">game where player is connected</param>
    Task StartPlayer(IGame game);

    /// <summary>
    /// Plays setup phase. Only one setup move is allowed.
    /// </summary>
    void PlaySetUp();

    /// <summary>
    /// Plays draft phase. Only draft and exchange card moves are allowed.
    /// </summary>
    void PlayDraft();

    /// <summary>
    /// Plays attack phase. Only attack and capture moves are allowed.
    /// Capture move can be made after captureing some area.
    /// </summary>
    void PlayAttack();

    /// <summary>
    /// Playes fortify phase. Only one fortify move is allowed.
    /// </summary>
    void PlayFortify();

    /// <summary>
    /// Asynchronously updates game plan.
    /// </summary>
    /// <param name="areaID">ID of area</param>
    /// <param name="armyColor">color of army</param>
    /// <param name="sizeOfArmy">size of army</param>
    /// <returns>async call</returns>
    Task UpdateGame(byte areaID, ArmyColor armyColor, int sizeOfArmy);

    /// <summary>
    /// Asynchronously ends player and notifies if player is winner.
    /// </summary>
    /// <param name="isWinner">if player is winner</param>
    Task EndPlayer(bool isWinner);
  }
}