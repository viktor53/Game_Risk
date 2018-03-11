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
using System.Windows;
using Risk.Model.GamePlan;
using Risk.Model.Enums;

namespace Risk.Networking.Client
{
  public class ConfirmationEventArgs : EventArgs
  {
    public bool Data { get; private set; }

    public ConfirmationEventArgs(bool data)
    {
      Data = data;
    }
  }

  public class InicializationEventArgs : EventArgs
  {
    public GameBoardInfo Data { get; private set; }

    public InicializationEventArgs(GameBoardInfo data)
    {
      Data = data;
    }
  }

  public class UpdateGameEventArgs : EventArgs
  {
    public Area Data { get; private set; }

    public UpdateGameEventArgs(Area data)
    {
      Data = data;
    }
  }

  public class FreeUnitEventArgs : EventArgs
  {
    public long Data { get; private set; }

    public FreeUnitEventArgs(long data)
    {
      Data = data;
    }
  }

  public class UpdateCardEventArgs : EventArgs
  {
    public bool Data { get; private set; }

    public UpdateCardEventArgs(bool data)
    {
      Data = data;
    }
  }

  public class ArmyColorEventArgs : EventArgs
  {
    public long Data { get; private set; }

    public ArmyColorEventArgs(long data)
    {
      Data = data;
    }
  }

  public class MoveResultEventArgs : EventArgs
  {
    public long Data { get; private set; }

    public MoveResultEventArgs(long data)
    {
      Data = data;
    }
  }

  public class RiskClient
  {
    private Socket _client;

    private IPEndPoint _remoteEP;

    private byte[] _buffer;

    private const int _bufferSize = 16384;

    private string _username;

    private object _receiveLock;

    private bool _listen;

    private Deserializer _deserializer;

    public event EventHandler OnUpdate;

    public event EventHandler OnConfirmation;

    public event EventHandler OnReadyTag;

    public event EventHandler OnInicialization;

    public event EventHandler OnYourTurn;

    private event EventHandler _onMoveresult;

    public event EventHandler OnMoveResult
    {
      add
      {
        _onMoveresult = null;
        _onMoveresult += value;
      }
      remove
      {
        _onMoveresult = null;
      }
    }

    public event EventHandler OnFreeUnit;

    public event EventHandler OnArmyColor;

    public event EventHandler OnUpdateCard;

    public event EventHandler OnEndGame;

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

    private IList<string> _players;

    public IList<string> Players => _players;

    private void AddPlayers(IList<string> names)
    {
      foreach (var name in names)
      {
        Players.Add(name);
      }
      OnUpdate?.Invoke(this, new EventArgs());
    }

    private void RemovePlayer(IList<string> names)
    {
      foreach (var name in names)
      {
        Players.Remove(name);
      }
      OnUpdate?.Invoke(this, new EventArgs());
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

      _deserializer = new Deserializer();

      _listen = false;

      _players = new List<string>();

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

    public async void StartListenToUpdates()
    {
      if (!_listen)
      {
        await Task.Run(() =>
        {
          _listen = true;
          while (_listen)
          {
            Message m = null;

            lock (_receiveLock)
            {
              m = ReceiveMessage();

              switch (m.MessageType)
              {
                case MessageType.UpdateGameList:
                  Rooms = _deserializer.GetData<IList<GameRoomInfo>>((JArray)m.Data);
                  break;

                case MessageType.UpdatePlayerListAdd:
                  AddPlayers(_deserializer.GetData<IList<string>>((JArray)m.Data));
                  break;

                case MessageType.UpdatePlayerListRemove:
                  RemovePlayer(_deserializer.GetData<IList<string>>((JArray)m.Data));
                  break;

                case MessageType.Confirmation:
                  OnConfirmation?.Invoke(this, new ConfirmationEventArgs((bool)m.Data));
                  break;

                case MessageType.InitializeGame:
                  OnInicialization?.Invoke(this, new InicializationEventArgs(_deserializer.DeserializeGameBoardInfo((JObject)m.Data)));
                  _listen = false;
                  break;

                default:
                  break;
              }
            }
          }
        });
      }
    }

    public async void ListenToGameCommands()
    {
      await Task.Run(() =>
      {
        bool end = false;
        while (!end)
        {
          Message m = ReceiveMessage();

          switch (m.MessageType)
          {
            case MessageType.YourTurn:
              OnYourTurn?.Invoke(this, new ConfirmationEventArgs((bool)m.Data));
              break;

            case MessageType.UpdateGame:
              OnUpdate?.Invoke(this, new UpdateGameEventArgs(_deserializer.DeserializeArea((JObject)m.Data)));
              break;

            case MessageType.FreeUnit:
              OnFreeUnit?.Invoke(this, new FreeUnitEventArgs((long)m.Data));
              break;

            case MessageType.MoveResult:
              _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)m.Data));
              break;

            case MessageType.ArmyColor:
              OnArmyColor?.Invoke(this, new ArmyColorEventArgs((long)m.Data));
              break;

            case MessageType.UpdateCard:
              OnUpdateCard?.Invoke(this, new UpdateCardEventArgs((bool)m.Data));
              break;

            case MessageType.EndGame:
              OnEndGame?.Invoke(this, new EventArgs());
              end = true;
              break;
          }
        }
      });
    }

    public async void StopListenToUpdates()
    {
      await Task.Run(() =>
      {
        _listen = false;
      });
    }

    public async Task<bool> SendConnectToGameRequest(string gameName)
    {
      return await Task.Run(() =>
      {
        Message mess = new Message(MessageType.ConnectToGame, gameName);

        SendMessage(mess);

        return true;
      });
    }

    public async Task<bool> SendCreateGameRequest(CreateGameRoomInfo roomInfo)
    {
      return await Task.Run(() =>
      {
        Message mess = new Message(MessageType.CreateGame, roomInfo);

        SendMessage(mess);

        return true;
      });
    }

    public async void SendLeaveGame()
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.Leave, null);
        SendMessage(mess);
        _players.Clear();
      });
    }

    public async void SendReadyTag()
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.ReadyTag, null);
        SendMessage(mess);
      });
    }

    public async void SendLougOut()
    {
      await Task.Run(() =>
      {
        _listen = false;
        Message mess = new Message(MessageType.Logout, null);
        SendMessage(mess);
      });
    }

    public async void SendSetUpMove(int idArea, ArmyColor playerColor)
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.SetUpMove, new SetUp(playerColor, idArea));
        SendMessage(mess);
      });
    }

    public async void SendNextPhase()
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.NextPhase, null);
        SendMessage(mess);
      });
    }

    public async void SendDraftMove(ArmyColor playerColor, int areaID, int numberOfUnit)
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.DraftMove, new Draft(playerColor, areaID, numberOfUnit));
        SendMessage(mess);
      });
    }

    public async void SendAttackMove(ArmyColor playerColor, int attackerAreaID, int defenderAreaID, AttackSize attackSize)
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.AttackMove, new Attack(playerColor, attackerAreaID, defenderAreaID, attackSize));
        SendMessage(mess);
      });
    }

    public async void SendFortifyMove(ArmyColor playerColor, int fromAreaID, int toAreaID, int sizeOfArmy)
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.FortifyMove, new Fortify(playerColor, fromAreaID, toAreaID, sizeOfArmy));
        SendMessage(mess);
      });
    }

    public async void SendCaptureMove(ArmyColor playerColor, int armyToMove)
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.CaptureMove, new Capture(playerColor, armyToMove));
        SendMessage(mess);
      });
    }

    public async void SendExchangeCard(ArmyColor playerColor)
    {
      await Task.Run(() =>
      {
        Message mess = new Message(MessageType.ExchangeCardsMove, playerColor);
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