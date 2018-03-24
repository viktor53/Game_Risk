using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Exceptions
{
  /// <summary>
  /// Exception when client received uknown response.
  /// </summary>
  public sealed class UnknownResponseException : Exception
  {
    /// <summary>
    /// Creates defualt UnknownResponseException.
    /// </summary>
    public UnknownResponseException() : base("Uknown response for the request. Try repeat it.")
    {
    }

    /// <summary>
    /// Creates UnknownResponseException with specific message.
    /// </summary>
    /// <param name="message">message of exception</param>
    public UnknownResponseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates UnknownResponseException with specific message and inner exception.
    /// </summary>
    /// <param name="message">message of exception</param>
    /// <param name="innerException">inner exception</param>
    public UnknownResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }
  }
}