using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Risk.Networking.Messages;
using Risk.Networking.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Networking.Messages.Data;
using System.Threading;

namespace Risk.Networking.Server
{
  internal class Player : IClientManager
  {
    private object _sendingLock;

    private Socket _connection;

    private const int _bufferSize = 4096;

    private byte[] _buffer;

    private JsonSerializer _serializer;

    private RiskServer _server;

    private IGame _game;

    private bool _listenToReady;

    private int _freeUnit;

    public IList<RiskCard> Cards { get; private set; }

    public int FreeUnit
    {
      get
      {
        return _freeUnit;
      }
      set
      {
        _freeUnit = value;
        Task.Run(() =>
        {
          SendMessage(new Message(MessageType.FreeUnit, _freeUnit));
        });
      }
    }

    public ArmyColor PlayerColor { get; set; }

    public string PlayerName { get; private set; }

    public IGameRoom GameRoom { get; set; }

    public event EventHandler OnReady;

    public event EventHandler OnLeave;

    private object _listeningLock;

    public Player(Socket connection, RiskServer server)
    {
      _connection = connection;
      _buffer = new byte[_bufferSize];
      _serializer = new JsonSerializer();
      _sendingLock = new object();
      _server = server;
      _listeningLock = new object();
      _listenToReady = false;
    }

    public void PlayAttack()
    {
      bool isNextPhase = false;
      while (!isNextPhase)
      {
        Message m = ReceiveMessage();
        switch (m.MessageType)
        {
          case MessageType.AttackMove:
            SendMoveResult(_game.MakeMove(GetData<Attack>((JObject)m.Data)));
            break;

          case MessageType.CaptureMove:
            SendMoveResult(_game.MakeMove(GetData<Capture>((JObject)m.Data)));
            break;

          case MessageType.NextPhase:
            isNextPhase = true;
            SendMoveResult(MoveResult.OK);
            break;

          default:
            SendErrorMessage();
            break;
        }
      }
    }

    public void PlayDraft()
    {
      SendYourTurnMessage(false);

      bool isNextPhase = false;
      while (!isNextPhase)
      {
        Message m = ReceiveMessage();
        switch (m.MessageType)
        {
          case MessageType.DraftMove:
            SendMoveResult(_game.MakeMove(GetData<Draft>((JObject)m.Data)));
            break;

          case MessageType.ExchangeCardsMove:
            SendMoveResult(_game.MakeMove(GetData<ExchangeCard>((JObject)m.Data)));
            break;

          case MessageType.NextPhase:
            isNextPhase = true;
            SendMoveResult(MoveResult.OK);
            break;

          default:
            SendErrorMessage();
            break;
        }
      }
    }

    public void PlayFortify()
    {
      bool isNextPhase = false;
      while (!isNextPhase)
      {
        Message m = ReceiveMessage();
        switch (m.MessageType)
        {
          case MessageType.FortifyMove:
            SendMoveResult(_game.MakeMove(GetData<Fortify>((JObject)m.Data)));
            break;

          case MessageType.NextPhase:
            isNextPhase = true;
            SendMoveResult(MoveResult.OK);
            break;

          default:
            SendErrorMessage();
            break;
        }
      }
    }

    public void PlaySetUp()
    {
      SendYourTurnMessage(true);

      bool isCorrect = false;
      while (!isCorrect)
      {
        Message m = ReceiveMessage();
        switch (m.MessageType)
        {
          case MessageType.SetUpMove:
            var jo = (JObject)m.Data;
            MoveResult result = _game.MakeMove(new SetUp((ArmyColor)GetData<long>(jo["PlayerColor"]), (int)GetData<long>(jo["AreaID"])));
            SendMoveResult(result);
            isCorrect = result == MoveResult.OK ? true : false;
            break;

          default:
            SendErrorMessage();
            break;
        }
      }
    }

    public Task UpdateGame(Area area)
    {
      return Task.Run(() =>
      {
        Message m = new Message(MessageType.UpdateGame, area);
        SendMessage(m);
      });
    }

    private T GetData<T>(JToken data)
    {
      using (JTokenReader reader = new JTokenReader(data))
      {
        return _serializer.Deserialize<T>(reader);
      }
    }

    private void SendYourTurnMessage(bool isSetUp)
    {
      Message m = new Message(MessageType.YourTurn, isSetUp);
      SendMessage(m);
    }

    private void SendMoveResult(MoveResult result)
    {
      Message m = new Message(MessageType.MoveResult, result);
      SendMessage(m);
    }

    private void SendErrorMessage()
    {
      Message m = new Message(MessageType.Error, new Error(ErrorType.UknownRequest, "Uknown or bad request!"));
      SendMessage(m);
    }

    private void SendMessage(Message m)
    {
      lock (_sendingLock)
      {
        _connection.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m)));
        Thread.Sleep(100);
      }
    }

    private Message ReceiveMessage()
    {
      int lengthOfData = _connection.Receive(_buffer);
      return JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengthOfData));
    }

    public void StartPlayer(IGame game)
    {
      _game = game;
      Message m = new Message(MessageType.InitializeGame, GameRoom.GetBoardInfo(_game.GetGamePlan()));
      Task.Run(() =>
      {
        SendMessage(m);

        SendMessage(new Message(MessageType.ArmyColor, PlayerColor));
      });
    }

    public void EndPlayer(bool isWinner)
    {
      Task.Run(() =>
      {
        Message m = new Message(MessageType.EndGame, isWinner);
        SendMessage(m);
      });
    }

    public Task SendNewPlayerConnected(string name)
    {
      return Task.Run(() =>
      {
        List<string> players = new List<string>();
        players.Add(name);
        Message m = new Message(MessageType.UpdatePlayerListAdd, players);
        SendMessage(m);
      });
    }

    public Task SendPlayerLeave(string name)
    {
      return Task.Run(() =>
      {
        List<string> players = new List<string>();
        players.Add(name);
        Message m = new Message(MessageType.UpdatePlayerListRemove, players);
        SendMessage(m);
      });
    }

    public Task SendConnectedPlayers(IList<string> names)
    {
      return Task.Run(() =>
      {
        Message m = new Message(MessageType.UpdatePlayerListAdd, names);
        SendMessage(m);
      });
    }

    public Task ListenToReadyTag()
    {
      if (!_listenToReady)
      {
        return Task.Run(() =>
        {
          _listenToReady = true;
          while (_listenToReady)
          {
            Message received = ReceiveMessage();
            switch (received.MessageType)
            {
              case MessageType.ReadyTag:
                OnReady?.Invoke(this, new EventArgs());
                _listenToReady = false;
                break;

              case MessageType.Leave:
                OnLeave?.Invoke(this, new EventArgs());
                SendUpdateGameList(_server.GetUpdateInfo());
                ManagingConnectingToRoom();
                _listenToReady = false;
                break;

              default:
                SendErrorMessage();
                break;
            }
          }
        });
      }
      return null;
    }

    public Task SartListening()
    {
      return Task.Run(() =>
      {
        bool isCorrect = false;
        while (!isCorrect)
        {
          Message m = ReceiveMessage();
          switch (m.MessageType)
          {
            case MessageType.Registration:
              if (_server.AddPlayer((string)m.Data, this))
              {
                PlayerName = (string)m.Data;
                SendMessage(new Message(MessageType.Confirmation, true));
                isCorrect = true;
              }
              else
              {
                SendMessage(new Message(MessageType.Confirmation, false));
              }
              break;

            case MessageType.Logout:
              return;

            default:
              SendErrorMessage();
              break;
          }
        }

        SendUpdateGameList(_server.GetUpdateInfo());

        ManagingConnectingToRoom();
      });
    }

    private async void ManagingConnectingToRoom()
    {
      await Task.Run(() =>
      {
        bool isInGameOrLeave = false;
        while (!isInGameOrLeave)
        {
          Message m = ReceiveMessage();
          switch (m.MessageType)
          {
            case MessageType.CreateGame:
              if (_server.CreateGame(GetData<CreateGameRoomInfo>((JObject)m.Data), PlayerName))
              {
                SendMessage(new Message(MessageType.Confirmation, true));

                isInGameOrLeave = true;
              }
              else
              {
                SendMessage(new Message(MessageType.Confirmation, false));
              }
              break;

            case MessageType.ConnectToGame:
              if (_server.ConnectToGame(PlayerName, (string)m.Data))
              {
                SendMessage(new Message(MessageType.Confirmation, true));

                isInGameOrLeave = true;
              }
              else
              {
                SendMessage(new Message(MessageType.Confirmation, false));
              }
              break;

            case MessageType.Logout:
              _server.LogOutPlayer(PlayerName);
              return;

            default:
              SendErrorMessage();
              break;
          }
        }

        SendConnectedPlayers(GameRoom.GetPlayers());
      });
    }

    public Task SendUpdateGameList(IList<GameRoomInfo> roomsInfo)
    {
      return Task.Run(() =>
      {
        Message m = new Message(MessageType.UpdateGameList, roomsInfo);
        SendMessage(m);
      });
    }
  }
}