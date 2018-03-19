using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;

namespace Risk.Model.GameCore
{
  /// <summary>
  /// Interface providing contract for game.
  /// </summary>
  public interface IGame
  {
    /// <summary>
    /// Adds new player into the game.
    /// </summary>
    /// <param name="player">new player</param>
    /// <returns>true if player is added, false if game is full</returns>
    bool AddPlayer(IPlayer player);

    /// <summary>
    /// Gets game plan (areas, connections).
    /// </summary>
    /// <returns>game plan (areas, connections)</returns>
    GamePlanInfo GetGamePlan();

    /// <summary>
    /// Starts game and plays until the end.
    /// </summary>
    void StartGame();

    /// <summary>
    /// Makes game move setup.
    /// </summary>
    /// <param name="move">setup move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, NotEnoughFreeUnit, NotYourArea, AlreadySetUpThisTurn)
    /// </returns>
    MoveResult MakeMove(SetUp move);

    /// <summary>
    /// Makes game move draft.
    /// </summary>
    /// <param name="move">draft move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, NotEnoughFreeUnit, NotYourArea)
    /// </returns>
    MoveResult MakeMove(Draft move);

    /// <summary>
    /// Makes game move exchange card.
    /// </summary>
    /// <param name="move">exchange card move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, InvalidCombination)
    /// </returns>
    MoveResult MakeMove(ExchangeCard move);

    /// <summary>
    /// Makes game move attack.
    /// </summary>
    /// <param name="move">attack move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, InvalidAttack, InvalidAttackerOrDefender, EmptyCapturedArea)
    /// </returns>
    MoveResult MakeMove(Attack move);

    /// <summary>
    /// Makes game move capture.
    /// </summary>
    /// <param name="move">capture move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, NoCapturedArea, InvalidNumberUnit)
    /// </returns>
    MoveResult MakeMove(Capture move);

    /// <summary>
    /// Makes game move fortify.
    /// </summary>
    /// <param name="move">fortify move command</param>
    /// <returns>
    ///   OK or corresponding error.
    ///   (NotYourTurn, BadPhase, AlreadyFortifyThisTurn, NotConnected, InvalidNumberUnit)
    /// </returns>
    MoveResult MakeMove(Fortify move);
  }
}