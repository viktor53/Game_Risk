using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Risk.Networking.Messages.Data;
using Risk.Networking.Enums;
using Risk.Networking.Messages;
using Risk.Networking.Exceptions;
using Risk.Model.GameCore.Moves;
using Risk.Model.Enums;

namespace Risk.Networking.Client
{
  /// <summary>
  /// Represents client side of risk player. Connets to risk server side.
  /// </summary>
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

    private IList<GameRoomInfo> _rooms;

    private IList<string> _players;

    private event EventHandler _onUpdate;

    private event EventHandler _onConfirmation;

    private event EventHandler _onInicialization;

    private event EventHandler _onYourTurn;

    private event EventHandler _onMoveresult;

    private event EventHandler _onFreeUnit;

    private event EventHandler _onArmyColor;

    private event EventHandler _onUpdateCard;

    private event EventHandler _onEndGame;

    public IList<GameRoomInfo> Rooms
    {
      get
      {
        return _rooms;
      }
      private set
      {
        _rooms = value;
        _onUpdate?.Invoke(this, new EventArgs());
      }
    }

    public IList<string> Players => _players;

    /// <summary>
    /// OnUpdate is raised whenever update is received.
    /// (update of game list, update of player list, update game board)
    /// </summary>
    public event EventHandler OnUpdate
    {
      add
      {
        _onUpdate = null;
        _onUpdate += value;
      }
      remove
      {
        _onUpdate -= value;
      }
    }

    /// <summary>
    /// OnConfirmation is raised whenever confirmation is received from server.
    /// </summary>
    public event EventHandler OnConfirmation
    {
      add
      {
        _onConfirmation = null;
        _onConfirmation += value;
      }
      remove
      {
        _onConfirmation -= value;
      }
    }

    /// <summary>
    /// OnInicialization is raised whenever game is inicializated.
    /// </summary>
    public event EventHandler OnInicialization
    {
      add
      {
        _onInicialization = null;
        _onInicialization += value;
      }
      remove
      {
        _onInicialization -= value;
      }
    }

    /// <summary>
    /// OnYourTurn is raised whenever player is playing now.
    /// </summary>
    public event EventHandler OnYourTurn
    {
      add
      {
        _onYourTurn = null;
        _onYourTurn += value;
      }
      remove
      {
        _onYourTurn -= value;
      }
    }

    /// <summary>
    /// OnMoveResult is raised whenever move result is reveived from game.
    /// </summary>
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

    /// <summary>
    /// OnFreeUnit is raised whenever number of free unit is changed.
    /// </summary>
    public event EventHandler OnFreeUnit
    {
      add
      {
        _onFreeUnit = null;
        _onFreeUnit += value;
      }
      remove
      {
        _onFreeUnit -= value;
      }
    }

    /// <summary>
    /// OnArmyColor is raised whenever player of color is changed.
    /// </summary>
    public event EventHandler OnArmyColor
    {
      add
      {
        _onArmyColor = null;
        _onArmyColor += value;
      }
      remove
      {
        _onArmyColor -= value;
      }
    }

    /// <summary>
    /// OnUpdateCard is raised whenever number of cards is changed.
    /// </summary>
    public event EventHandler OnUpdateCard
    {
      add
      {
        _onUpdateCard = null;
        _onUpdateCard += value;
      }
      remove
      {
        _onUpdateCard -= value;
      }
    }

    /// <summary>
    /// OnEndGame is raised whenever game is ended.
    /// </summary>
    public event EventHandler OnEndGame
    {
      add
      {
        _onEndGame = null;
        _onEndGame += value;
      }
      remove
      {
        _onEndGame -= value;
      }
    }

    /// <summary>
    /// Default constructor that connects to localhost on port 11000.
    /// </summary>
    public RiskClient() : this("localhost", 11000)
    {
    }

    /// <summary>
    /// Constructor can specify server address and have default port 11000.
    /// </summary>
    /// <param name="hostNameOrAddressServer">host name of server or its address</param>
    public RiskClient(string hostNameOrAddressServer) : this(hostNameOrAddressServer, 11000)
    {
    }

    /// <summary>
    /// Constructor with all parameters.
    /// </summary>
    /// <param name="hostNameOrAddressServer">host name of server or its address</param>
    /// <param name="port">port of server</param>
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
    }

    /// <summary>
    /// Adds players to list of players.
    /// </summary>
    /// <param name="names">list of new players</param>
    private void AddPlayers(IEnumerable<string> names)
    {
      foreach (var name in names)
      {
        Players.Add(name);
      }
      _onUpdate?.Invoke(this, new EventArgs());
    }

    /// <summary>
    /// Removes players from list of players.
    /// </summary>
    /// <param name="names"></param>
    private void RemovePlayers(IEnumerable<string> names)
    {
      foreach (var name in names)
      {
        Players.Remove(name);
      }
      _onUpdate?.Invoke(this, new EventArgs());
    }

    /// <summary>
    /// Asynchronously connects to server.
    /// </summary>
    /// <returns>true if connecting succeeded, otherwise false</returns>
    public async Task<bool> ConnectAsync()
    {
      return await Task.Run(() =>
      {
        try
        {
          _client.Connect(_remoteEP);
          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends registration request.
    /// </summary>
    /// <param name="name">name to be registered</param>
    /// <returns>true if sending succeeded, otherwise false</returns>
    public async Task<bool> SendRegistrationRequestAsync(string name)
    {
      return await Task.Run(() =>
      {
        Message m = new Message(MessageType.Registration, name);
        SendMessage(m);

        m = ReceiveMessage();

        if (m.MessageType == MessageType.Confirmation)
        {
          if ((bool)m.Data)
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
          ProcessError(m);
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously listens to updates of game list and player list.
    /// </summary>
    /// <returns>true if listening succeeded, otherwise false</returns>
    public async Task<bool> ListenToUpdatesAsync()
    {
      return await Task.Run(() =>
      {
        if (!_listen)
        {
          _listen = true;
          while (_listen)
          {
            try
            {
              Message m = ReceiveMessage();

              switch (m.MessageType)
              {
                case MessageType.UpdateGameList:
                  Rooms = _deserializer.GetData<IList<GameRoomInfo>>((JArray)m.Data);
                  break;

                case MessageType.UpdatePlayerListAdd:
                  AddPlayers(_deserializer.GetData<IList<string>>((JArray)m.Data));
                  break;

                case MessageType.UpdatePlayerListRemove:
                  RemovePlayers(_deserializer.GetData<IList<string>>((JArray)m.Data));
                  break;

                case MessageType.Confirmation:
                  _onConfirmation?.Invoke(this, new ConfirmationEventArgs((bool)m.Data));
                  break;

                case MessageType.InitializeGame:
                  _onInicialization?.Invoke(this, new InicializationEventArgs(_deserializer.DeserializeGameBoardInfo((JObject)m.Data)));
                  _players.Clear();
                  _listen = false;
                  break;

                default:
                  break;
              }
            }
            catch (SocketException)
            {
              return false;
            }
          }
        }
        return true;
      });
    }

    /// <summary>
    /// Asynchronously listents to game commands. (Update state of game, player's turn, move result)
    /// </summary>
    /// <returns>true if listening succeeded, otherwise false</returns>
    public async Task<bool> ListenToGameCommandsAsync()
    {
      return await Task.Run(() =>
      {
        bool end = false;
        while (!end)
        {
          try
          {
            Message m = ReceiveMessage();

            switch (m.MessageType)
            {
              case MessageType.YourTurn:
                _onYourTurn?.Invoke(this, new ConfirmationEventArgs((bool)m.Data));
                break;

              case MessageType.UpdateGame:
                _onUpdate?.Invoke(this, new UpdateGameEventArgs(_deserializer.DeserializeArea((JObject)m.Data)));
                break;

              case MessageType.FreeUnit:
                _onFreeUnit?.Invoke(this, new FreeUnitEventArgs((long)m.Data));
                break;

              case MessageType.MoveResult:
                _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)m.Data));
                break;

              case MessageType.ArmyColor:
                _onArmyColor?.Invoke(this, new ArmyColorEventArgs((long)m.Data));
                break;

              case MessageType.UpdateCard:
                _onUpdateCard?.Invoke(this, new UpdateCardEventArgs((bool)m.Data));
                break;

              case MessageType.EndGame:
                _onEndGame?.Invoke(this, new EndGameEventArgs((bool)m.Data));
                end = true;
                break;
            }
          }
          catch (SocketException)
          {
            return false;
          }
        }

        return true;
      });
    }

    /// <summary>
    /// Asynchronously sends connect to the game request.
    /// </summary>
    /// <param name="gameName">name of the game</param>
    /// <returns>true if sending succeeded, otherwise false</returns>
    public async Task<bool> SendConnectToGameRequestAsync(string gameName)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.ConnectToGame, gameName);
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends create game request.
    /// </summary>
    /// <param name="roomInfo">information about creating game</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendCreateGameRequestAsync(CreateGameRoomInfo roomInfo)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.CreateGame, roomInfo);
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends leave game request.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendLeaveGameRequestAsync()
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.Leave, null);
          SendMessage(m);

          _players.Clear();

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends ready tag request.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendReadyTagRequestAsync()
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.ReadyTag, null);
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends logout request.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendLougOutRequestAsync()
    {
      return await Task.Run(() =>
      {
        try
        {
          _listen = false;

          Message m = new Message(MessageType.Logout, null);
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends setup move.
    /// </summary>
    /// <param name="idArea">id of area, where one unit will be placed</param>
    /// <param name="playerColor">color of player</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendSetUpMoveAsync(int idArea, ArmyColor playerColor)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.SetUpMove, new SetUp(playerColor, idArea));
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends next phase.
    /// </summary>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendNextPhaseAsync()
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.NextPhase, null);
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends draft move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="areaID">id of area, where units will be placed</param>
    /// <param name="numberOfUnit">number of units</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendDraftMoveAsync(ArmyColor playerColor, int areaID, int numberOfUnit)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.DraftMove, new Draft(playerColor, areaID, numberOfUnit));
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends attack move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="attackerAreaID">id of area, where attack comes from</param>
    /// <param name="defenderAreaID">id of area, where attack goes</param>
    /// <param name="attackSize">size of attack</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendAttackMoveAsync(ArmyColor playerColor, int attackerAreaID, int defenderAreaID, AttackSize attackSize)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.AttackMove, new Attack(playerColor, attackerAreaID, defenderAreaID, attackSize));
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends fortify move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="fromAreaID">id of area, where army comes from</param>
    /// <param name="toAreaID">id of area, where army goes</param>
    /// <param name="sizeOfArmy">size of army</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendFortifyMoveAsync(ArmyColor playerColor, int fromAreaID, int toAreaID, int sizeOfArmy)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.FortifyMove, new Fortify(playerColor, fromAreaID, toAreaID, sizeOfArmy));
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends capture move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="armyToMove">army to move</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendCaptureMoveAsync(ArmyColor playerColor, int armyToMove)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.CaptureMove, new Capture(playerColor, armyToMove));
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Asynchronously sends exchange card move.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <returns>true if sending succeded, otherwise false</returns>
    public async Task<bool> SendExchangeCardAsync(ArmyColor playerColor)
    {
      return await Task.Run(() =>
      {
        try
        {
          Message m = new Message(MessageType.ExchangeCardsMove, playerColor);
          SendMessage(m);

          return true;
        }
        catch (SocketException)
        {
          return false;
        }
      });
    }

    /// <summary>
    /// Sends the message.
    /// </summary>
    /// <param name="m">message</param>
    private void SendMessage(Message m)
    {
      _client.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m)));
    }

    /// <summary>
    /// Receives message.
    /// </summary>
    /// <returns>received message</returns>
    private Message ReceiveMessage()
    {
      int lengtOfData = _client.Receive(_buffer);
      return JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengtOfData));
    }

    /// <summary>
    /// Processes error message or unknown message.
    /// </summary>
    /// <param name="message">received message</param>
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