using Risk.Model.GamePlan;

namespace Risk.Model.Factories
{
  /// <summary>
  /// Provides contract to be game board factory.
  /// </summary>
  internal interface IGameBoardFactory
  {
    /// <summary>
    /// Creates new game board.
    /// </summary>
    /// <returns>new game board</returns>
    GameBoard CreateGameBoard();

    /// <summary>
    /// Creates new game board.
    /// </summary>
    /// <param name="numberOfAreas">number of areas on game board</param>
    /// <returns>new game board</returns>
    GameBoard CreateGameBoard(int numberOfAreas);
  }
}