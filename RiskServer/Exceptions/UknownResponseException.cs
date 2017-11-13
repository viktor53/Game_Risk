using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Exceptions
{
  public sealed class UknownResponseException: Exception
  {
    public UknownResponseException(): base("Uknown response for the request. Try repeat it.") { }

    public UknownResponseException(string message): base(message) { }

    public UknownResponseException(string message, Exception innerException): base(message, innerException) { }
  }
}
