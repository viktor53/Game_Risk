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

namespace Risk.Networking.Server
{
  internal class Player : IClientManager
  {
    private object _sendingLock;

    private Socket _connection;

    private const int _bufferSize = 1024;

    private byte[] _buffer;

    private JsonSerializer _serializer;

    private RiskServer _server;

    public IGame _game;

    public IList<RiskCard> Cards { get; private set; }

    public int FreeUnit { get; set; }

    public ArmyColor PlayerColor { get; set; }

    public string PlayerName { get; private set; }

    public IGameRoom GameRoom { get; set; }

    public Player(Socket connection, RiskServer server)
    {
      _connection = connection;
      _buffer = new byte[_bufferSize];
      _serializer = new JsonSerializer();
      _sendingLock = new object();
      _server = server;
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
      SendYourTurnMessage();

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
      SendYourTurnMessage();

      bool isCorrect = false;
      while (!isCorrect)
      {
        Message m = ReceiveMessage();
        switch (m.MessageType)
        {
          case MessageType.SetUpMove:
            MoveResult result = _game.MakeMove(GetData<SetUp>((JObject)m.Data));
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

    private T GetData<T>(JObject data)
    {
      using (JTokenReader reader = new JTokenReader(data))
      {
        return _serializer.Deserialize<T>(reader);
      }
    }

    private void SendYourTurnMessage()
    {
      Message m = new Message(MessageType.YourTurn, null);
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

    public Task<bool> WaitUntilPlayerIsReady()
    {
      return Task.Run(() =>
      {
        Message m = new Message(MessageType.ReadyTag, null);
        SendMessage(m);

        while (true)
        {
          Message received = ReceiveMessage();
          switch (received.MessageType)
          {
            case MessageType.ReadyTag:
              return true;

            case MessageType.Leave:
              return false;

            default:
              SendErrorMessage();
              break;
          }
        }
      });
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