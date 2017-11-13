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

namespace Risk.Networking.Client
{
  public class RiskClient
  {
    private Socket _client;

    private IPEndPoint _remoteEP;

    private ManualResetEvent _sendDone;

    private ManualResetEvent _receiveDone;

    private byte[] _buffer;

    private const int _bufferSize = 512;

    private int _size;

    private string _username;

    private Dictionary<string, PlayerInfo> _players;

    private Dictionary<string, GameInfo> _games;

    private IRequestFactory _requestFactory;

    public RiskClient(): this("localhost", 11000, new RequestFactory()) { }

    public RiskClient(string hostNameOrAddressServer): this(hostNameOrAddressServer, 11000, new RequestFactory()) { }

    public RiskClient(string hostNameOrAddressServer, int port, IRequestFactory requestFactory)
    {
      IPAddress ipAddress = Dns.GetHostEntry(hostNameOrAddressServer).AddressList[0];
      _remoteEP = new IPEndPoint(ipAddress, port);

      _client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      _buffer = new byte[_bufferSize];

      _requestFactory = requestFactory;

      _sendDone = new ManualResetEvent(false);
      _receiveDone = new ManualResetEvent(false);

      _players = new Dictionary<string, PlayerInfo>();
      _games = new Dictionary<string, GameInfo>();

      Debug.WriteLine("**Client inicialization: OK");
    }


    public async void ConnectAsync()
    {
      Task connecting =  new Task(() => _client.Connect(_remoteEP));
      connecting.Start();
      await connecting;
    }

    public void Start()
    {
      try
      {
        _client.Connect(_remoteEP);

        SendRegistrationRequestAsync("Haha");
        _sendDone.WaitOne();
        _sendDone.Reset();

        Receive(_client);
        _receiveDone.WaitOne();
        _receiveDone.Reset();

        Thread.Sleep(3000);

        Send(_client, "Test2<EOF>");
        _sendDone.WaitOne();
        _sendDone.Reset();

        Receive(_client);
        _receiveDone.WaitOne();
        _receiveDone.Reset();

        _client.Shutdown(SocketShutdown.Both);
        _client.Close();
      }
      catch(Exception e)
      {
        Debug.WriteLine("**Client ERROR: " + e.StackTrace);
      }
    }

    private void Send(Socket client, string data)
    {
      byte[] byteData = Encoding.ASCII.GetBytes(data);

      client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);

      Debug.WriteLine("**Client sending data: BEGIN");
    }

    private void SendCallback(IAsyncResult result)
    {
      try
      {
        Socket client = (Socket)result.AsyncState;

        int byteSent = client.EndSend(result);

        _sendDone.Set();

        Debug.WriteLine("**Client sending data: END");
      }
      catch(Exception e)
      {
        Debug.WriteLine("**Client ERROR: " + e.StackTrace);
      }
    }

    private void Receive(Socket client)
    {
      try
      {
        Player player = new Player();
        player.connection = client;

        client.BeginReceive(player.buffer, 0, Player.bufferSize, 0, new AsyncCallback(ReceiveCallback), player);

        Debug.WriteLine("**Client receiving data: BEGIN");
      }
      catch(Exception e)
      {
        Debug.WriteLine("**Client ERROR: " + e.StackTrace);
      }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
      try
      {
        Player state = (Player)result.AsyncState;
        Socket client = state.connection;

        int bytesRead = client.EndReceive(result);

        if (bytesRead > 0)
        {
          state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

          client.BeginReceive(state.buffer, 0, Player.bufferSize, 0, new AsyncCallback(ReceiveCallback), state);

          Debug.WriteLine("**Client receiving data: END");
        }
        else
        {
          if (state.sb.Length > 1)
          {
            Debug.WriteLine(state.sb.ToString());
          }

          _receiveDone.Set();

          Debug.WriteLine("**Client receiving data: END");
        }
      }
      catch (Exception e)
      {
        Debug.WriteLine("**Client ERROR: " + e.StackTrace);
      }
    }

    private bool IsRegistred()
    {
      return _username != null;
    }

    public async Task<bool> SendRegistrationRequestAsync(string name)
    {
      Task<bool> sending = new Task<bool>(() => {
        string message = JsonConvert.SerializeObject(_requestFactory.CreateRegistrationRequest(name));
        _client.Send(Encoding.ASCII.GetBytes(message));

        _size = _client.Receive(_buffer);
        message = Encoding.ASCII.GetString(_buffer, 0, _size);
        Message regResponse = JsonConvert.DeserializeObject<Message>(message);

        if (regResponse.MessageType == MessageType.Confirmation)
        {
          if((bool)regResponse.Data)
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
          ProcessError(regResponse);
          return false;
        }
      });

      sending.Start();

      return await sending;
    }

    public async void SendUpdateRequestAsync()
    {
      Task sending = new Task(() =>
      {
        string message = JsonConvert.SerializeObject(_requestFactory.CreateUpdateRequest(_username));
        _client.Send(Encoding.ASCII.GetBytes(message));

        _size = _client.Receive(_buffer);
        message = Encoding.ASCII.GetString(_buffer, 0, _size);
        Message upResponese = JsonConvert.DeserializeObject<Message>(message);

        if(upResponese.MessageType == MessageType.Update)
        {
          UpdateInfo updateInfo = JsonConvert.DeserializeObject<UpdateInfo>((string)upResponese.Data);
          foreach(PlayerInfo playerInfo in updateInfo.Players)
          {
            _players.Add(playerInfo.Username, playerInfo);
          }
          foreach(GameInfo gameInfo in updateInfo.Games)
          {
            _games.Add(gameInfo.GameName, gameInfo);
          }
        }
        else
        {
          ProcessError(upResponese);
        }
      });

      sending.Start();

      await sending;
    }

    public async Task<bool> SendCreateGameRequest(string gameName, int numberOfPlayers)
    {
      Task<bool> sending = new Task<bool>(() =>
      {
        string message = JsonConvert.SerializeObject(_requestFactory.CreateCreateGameRequest(_username, gameName, numberOfPlayers));
        _client.Send(Encoding.ASCII.GetBytes(message));

        _size = _client.Receive(_buffer);
        message = Encoding.ASCII.GetString(_buffer, 0, _size);
        Message createGameResponese = JsonConvert.DeserializeObject<Message>(message);

        if(createGameResponese.MessageType == MessageType.Confirmation)
        {
          return (bool)createGameResponese.Data;
        }
        else
        {
          ProcessError(createGameResponese);
          return false;
        }
      });

      sending.Start();

      return await sending;
    }

    public async Task<bool> SendConnectToGameReqeust(string gameName)
    {
      Task<bool> sending = new Task<bool>(() =>
      {
        string message = JsonConvert.SerializeObject(_requestFactory.CreateConnectToGameRequest(_username, gameName));
        _client.Send(Encoding.ASCII.GetBytes(message));

        _size = _client.Receive(_buffer);
        message = Encoding.ASCII.GetString(_buffer, 0, _size);
        Message connectResponse = JsonConvert.DeserializeObject<Message>(message);

        if (connectResponse.MessageType == MessageType.Confirmation)
        {
          return (bool)connectResponse.Data;
        }
        else
        {
          ProcessError(connectResponse);
          return false;
        }
      });

      sending.Start();

      return await sending;
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
