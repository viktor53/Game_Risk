namespace Risk.Model.Enums
{
  /// <summary>
  /// Represents game move result.
  /// </summary>
  public enum MoveResult
  {
    /// <summary>
    /// Success
    /// </summary>

    OK,
    Winner,
    AreaCaptured,

    /// <summary>
    /// Error
    /// </summary>

    BadPhase,
    NotYourTurn,
    AlreadySetUpThisTurn,
    AlreadyFortifyThisTurn,
    NotEnoughFreeUnit,
    NotYourArea,
    InvalidAttackerOrDefender,
    InvalidAttack,
    EmptyCapturedArea,
    NoCapturedArea,
    InvalidNumberUnit,
    NotConnected,
    InvalidCombination
  }
}