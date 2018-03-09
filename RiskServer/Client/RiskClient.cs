using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using Risk.Networking.Messages.Data;
using Risk.Networking.Enums;
using Risk.Networking.Messages;
using Risk.Networking.Exceptions;
using Risk.Model.GameCore.Moves;
using Newtonsoft.Json.Linq;

namespace Risk.Networking.Client
{
  public class RiskClient
  {
    private Socket _client;

    private IPEndPoint _remoteEP;

    private byte[] _buffer;

    private const int _bufferSize = 1024;

    private string _username;

    private object _receiveLock;

    private bool _listen;

    private JsonSerializer _serializer;

    public event EventHandler OnUpdate;

    private IList<GameRoomInfo> _rooms;

    public IList<GameRoomInfo> Rooms
    {
      get
      {
        return _rooms;
      }
      private set
      {
        _rooms = value;
        OnUpdate?.Invoke(this, new EventArgs());
      }
    }

    public RiskClient() : this("localhost", 11000)
    {
    }

    public RiskClient(string hostNameOrAddressServer) : this(hostNameOrAddressServer, 11000)
    {
    }

    public RiskClient(string hostNameOrAddressServer, int port)
    {
      IPAddress ipAddress = Dns.GetHostEntry(hostNameOrAddressServer).AddressList[0];
      _remoteEP = new IPEndPoint(ipAddress, port);

      _client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      _buffer = new byte[_bufferSize];

      _receiveLock = new object();

      _serializer = new JsonSerializer();

      Debug.WriteLine("**Client inicialization: OK");
    }

    public async void ConnectAsync()
    {
      await Task.Run(() => _client.Connect(_remoteEP));
    }

    public async Task<bool> SendRegistrationRequestAsync(string name)
    {
      return await Task.Run(() =>
      {
        Message mess = new Message(MessageType.Registration, name);
        SendMessage(mess);

        mess = ReceiveMessage();

        if (mess.MessageType == MessageType.Confirmation)
        {
          if ((bool)mess.Data)
          {
            _username = name;
            return true;
          }
          else
          {
            return false;
          }
        }
        else
        {
          ProcessError(mess);
          return false;
        }
      });
    }

    private void ListenToUpdates()
    {
      while (_listen)
      {
        Message m;

        lock (_receiveLock)
        {
          m = ReceiveMessage();
        }

        if (m.MessageType == MessageType.UpdateGameList)
        {
          IList<GameRoomInfo> roomsInfo = GetData<IList<GameRoomInfo>>((JObject)m.Data);
        }
      }
    }

    private T GetData<T>(JObject data)
    {
      using (JTokenReader reader = new JTokenReader(data))
      {
        return _serializer.Deserialize<T>(reader);
      }
    }

    public async Task<bool> SendConnectToGameRequest(string gameName)
    {
      return await Task.Run(() =>
      {
        Message mess = new Message(MessageType.ConnectToGame, gameName);
        _client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(mess)));

        mess = ReceiveMessage();

        if (mess.MessageType == MessageType.Confirmation)
        {
          return (bool)mess.Data;
        }
        else
        {
          ProcessError(mess);
          return false;
        }
      });
    }

    public async Task<bool> SendCreateGameRequest(CreateGameRoomInfo roomInfo)
    {
      return await Task.Run(() =>
      {
        Message mess = new Message(MessageType.CreateGame, roomInfo);
        SendMessage(mess);

        mess = ReceiveMessage();

        if (mess.MessageType == MessageType.Confirmation)
        {
          return (bool)mess.Data;
        }
        else
        {
          ProcessError(mess);
          return false;
        }
      });
    }

    public async void SendLougOut()
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.Logout, null);
        SendMessage(mess);
      });
    }

    private void SendMessage(Message mess)
    {
      _client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(mess)));
    }

    private Message ReceiveMessage()
    {
      int lengtOfData = _client.Receive(_buffer);
      return JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengtOfData));
    }

    private bool IsRegistred()
    {
      return _username != null;
    }

    private void ProcessError(Message message)
    {
      if (message.MessageType == MessageType.Error)
      {
        Error error = JsonConvert.DeserializeObject<Error>((string)message.Data);
        throw new ServerErrorException(error);
      }
      else
      {
        throw new UknownResponseException();
      }
    }
  }
}