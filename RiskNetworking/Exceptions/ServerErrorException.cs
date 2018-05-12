using System;
using Risk.Networking.Messages.Data;
using Risk.Networking.Enums;

namespace Risk.Networking.Exceptions
{
  /// <summary>
  /// Exception when some error occurs on server side.
  /// </summary>
  public sealed class ServerErrorException : Exception
  {
    /// <summary>
    /// Creates ServerErrorException with message containing error type and detailed message.
    /// </summary>
    /// <param name="error">occured error</param>
    public ServerErrorException(Error error) : base($"Error type: {error.ErrorType} \n Detailed message: {error.DetailedMessage}")
    {
    }

    /// <summary>
    /// Creates ServerErrorException with message containing error type and detailed message.
    /// </summary>
    /// <param name="errorType">type of error</param>
    /// <param name="message">detailed message about the error</param>
    public ServerErrorException(ErrorType errorType, string message) : base($"Error type: {errorType} \n Detailed message: {message}")
    {
    }

    /// <summary>
    /// Creates ServerErrorException with message containing error type, detailed message and inner exception.
    /// </summary>
    /// <param name="error">occured error</param>
    /// <param name="innerException">inner exception</param>
    public ServerErrorException(Error error, Exception innerException) : base($"Error type: {error.ErrorType} \n Detailed message: {error.DetailedMessage}", innerException)
    {
    }

    /// <summary>
    /// Creates ServerErrorException with message containing error type, detailed message and inner exception.
    /// </summary>
    /// <param name="errorType">type of error</param>
    /// <param name="message">detailed message about error</param>
    /// <param name="innerException">inner exception</param>
    public ServerErrorException(ErrorType errorType, string message, Exception innerException) : base($"Error type: {errorType} \n Detailed message: {message}", innerException)
    {
    }
  }
}