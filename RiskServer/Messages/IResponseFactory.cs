using Risk.Networking.Messages.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Messages
{
  public interface IResponseFactory
  {
    IMessage CreateConfirmationResponse(bool result);

    IMessage CreateErrorResponse(Error error);

    IMessage CreateUpdateResponse(UpdateInfo updateInfo);

    IMessage CreateUpdateDifference(UpdateDifference updateInfo);
  }
}
