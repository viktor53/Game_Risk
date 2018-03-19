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
  }
}