using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Messages.Data;
using Risk.Networking.Enums;

namespace Risk.Networking.Exceptions
{
  public sealed class ServerErrorException: Exception
  {
    public ServerErrorException(Error error): base($"Error type: {error.ErrorType} \n Detailed message: {error.DetailedMessage}") { }

    public ServerErrorException(ErrorType errorType, string message): base($"Error type: {errorType} \n Detailed message: {message}") { }

    public ServerErrorException(Error error, Exception innerException): base($"Error type: {error.ErrorType} \n Detailed message: {error.DetailedMessage}", innerException) { }

    public ServerErrorException(ErrorType errorType, string message, Exception innerException): base($"Error type: {errorType} \n Detailed message: {message}", innerException) { }
  }
}
