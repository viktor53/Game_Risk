using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Risk.Networking.Messages;
using Risk.Networking.Enums;
using Risk.Networking.Messages.Data;

namespace Risk.Networking.Server
{
  class Client
  {
    private Socket _handler;

    private RiskServer _server;

    private const int _bufferSize = 1024;

    private byte[] _buffer;

    private IResponseFactory _responseFactory;

    private string _uknownReqResponse;

    public Client(Socket handler, RiskServer server, IResponseFactory responseFactory)
    {
      _handler = handler;
      _server = server;
      _buffer = new byte[_bufferSize];
      _responseFactory = responseFactory;
      _uknownReqResponse = JsonConvert.SerializeObject(_responseFactory.CreateErrorResponse(new Error(Enums.ErrorType.UknownRequest, "Uknown request. It is bad request or it does not know.")));
    }

    public async void StartAsync()
    {
      Task handling = new Task(() => {
        string response = null;

        Message message = null;

        message = WaitForTypeMessage(MessageType.Registration);

        response = JsonConvert.SerializeObject(_responseFactory.CreateConfirmationResponse(false));
        while (_server.Players.Contains((string)message.Data))
        {
          _handler.Send(Encoding.ASCII.GetBytes(response));

          message = WaitForTypeMessage(MessageType.Registration);
        }

        response = JsonConvert.SerializeObject(_responseFactory.CreateConfirmationResponse(true));
        _handler.Send(Encoding.ASCII.GetBytes(response));
        _server.Players.Add((string)message.Data);

        while (true)
        {
          
        }

      });

      handling.Start();

      await handling;
    }


    private Message WaitForTypeMessage(MessageType messageType)
    {
      int bytes = _handler.Receive(_buffer);
      string request = Encoding.ASCII.GetString(_buffer, 0, bytes);
      Message message = JsonConvert.DeserializeObject<Message>(request);

      while (message.MessageType != messageType)
      {
        _handler.Send(Encoding.ASCII.GetBytes(request));

        bytes = _handler.Receive(_buffer);
        request = Encoding.ASCII.GetString(_buffer, 0, bytes);
        message = JsonConvert.DeserializeObject<Message>(request);
      }

      return message;
    }
  }
}
