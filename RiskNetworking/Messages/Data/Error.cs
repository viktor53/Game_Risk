using Risk.Networking.Enums;

namespace Risk.Networking.Messages.Data
{
  /// <summary>
  /// Message data representing error.
  /// </summary>
  public sealed class Error
  {
    /// <summary>
    /// Type of error that happened.
    /// </summary>
    public ErrorType ErrorType { get; set; }

    /// <summary>
    /// Detailed message about the error.
    /// </summary>
    public string DetailedMessage { get; set; }

    /// <summary>
    /// Creates information about error.
    /// </summary>
    /// <param name="errorType">type of error</param>
    /// <param name="detailedMessage">detailed message about error</param>
    public Error(ErrorType errorType, string detailedMessage)
    {
      ErrorType = errorType;
      DetailedMessage = detailedMessage;
    }
  }
}