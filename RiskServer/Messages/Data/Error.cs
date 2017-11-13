using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Enums;

namespace Risk.Networking.Messages.Data
{
  public sealed class Error
  {
    public ErrorType ErrorType { get; set; }

    public string DetailedMessage { get; set; }

    public Error(ErrorType errorType, string detailedMessage)
    {
      ErrorType = errorType;
      DetailedMessage = detailedMessage;
    }
  }
}
