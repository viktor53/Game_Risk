using System;
using System.Threading.Tasks;
using Risk.Model.Enums;

namespace Risk.Networking.Client
{
  /// <summary>
  /// Provides contract for player on client side.
  /// </summary>
  public interface IPlayer
  {
    /// <summary>
    /// OnUpdate is raised whenever update of game board is received.
    /// </summary>
    event EventHandler OnUpdate;

    /// <summary>
    /// OnArmyColor is raised whenever player of color is changed.
    /// </summary>
    event EventHandler OnArmyColor;

    /// <summary>
    /// OnFreeUnit is raised whenever number of free unit is changed.
    /// </summary>
    event EventHandler OnFreeUnit;

    /// <summary>
    /// OnYourTurn is raised whenever player is playing now.
    /// </summary>
    event EventHandler OnYourTurn;

    /// <summary>
    /// OnUpdateCard is raised whenever number of cards is changed.
    /// </summary>
    event EventHandler OnUpdateCard;

    /// <summary>
    /// OnMoveResult is raised whenever move result is reveived from game.
    /// </summary>
    event EventHandler OnMoveResult;

    /// <summary>
    /// OnEndGame is raised whenever game is ended.
    /// </summary>
    event EventHandler OnEndGame;

    /// <summary>
    /// Asynchronously listents to game commands. (Update state of game, player's turn, move result)
    /// </summary>
    /// <returns>true if listening succeeded, otherwise false</returns>
    Task<bool> ListenToGameCommandsAsync();

    /// <summary>
    /// Asynchronously sends next phase.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendNextPhaseAsync();

    /// <summary>
    /// Asynchronously sends setup move.
    /// </summary>
    /// <param name="idArea">id of area, where one unit will be placed</param>
    /// <param name="playerColor">color of player</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendSetUpMoveAsync(ArmyColor playerColor, int idArea);

    /// <summary>
    /// Asynchronously sends draft move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="areaID">id of area, where units will be placed</param>
    /// <param name="numberOfUnit">number of units</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendDraftMoveAsync(ArmyColor playerColor, int areaID, int numberOfUnit);

    /// <summary>
    /// Asynchronously sends exchange card move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendExchangeCardAsync(ArmyColor playerColor);

    /// <summary>
    /// Asynchronously sends attack move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="attackerAreaID">id of area, where attack comes from</param>
    /// <param name="defenderAreaID">id of area, where attack goes</param>
    /// <param name="attackSize">size of attack</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendAttackMoveAsync(ArmyColor playerColor, int attackerAreaID, int defenderAreaID, AttackSize attackSize);

    /// <summary>
    /// Asynchronously sends capture move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="armyToMove">army to move</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendCaptureMoveAsync(ArmyColor playerColor, int armyToMove);

    /// <summary>
    /// Asynchronously sends fortify move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="fromAreaID">id of area, where army comes from</param>
    /// <param name="toAreaID">id of area, where army goes</param>
    /// <param name="sizeOfArmy">size of army</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    Task<bool> SendFortifyMoveAsync(ArmyColor playerColor, int fromAreaID, int toAreaID, int sizeOfArmy);
  }
}