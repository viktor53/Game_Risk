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

    private Deserializer _deserializer;

    private RiskServer _server;

    private IGame _game;

    private bool _listenToReady;

    private int _freeUnit;

    private IList<RiskCard> _cards;

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
      _deserializer = new Deserializer();
      _sendingLock = new object();
      _server = server;
      _listeningLock = new object();
      _listenToReady = false;
      _cards = new List<RiskCard>();
    }

    public void AddCard(RiskCard card)
    {
      _cards.Add(card);
      Task.Run(() =>
      {
        SendMessage(new Message(MessageType.UpdateCard, true));
      });
    }

    public void RemoveCard(RiskCard card)
    {
      _cards.Remove(card);
      Task.Run(() =>
      {
        SendMessage(new Message(MessageType.UpdateCard, false));
      });
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
            SendMoveResult(_game.MakeMove(_deserializer.DeserializeAttackMove((JObject)m.Data)));
            break;

          case MessageType.CaptureMove:
            SendMoveResult(_game.MakeMove(_deserializer.DeserilizeCaptureMove((JObject)m.Data)));
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
            SendMoveResult(_game.MakeMove(_deserializer.DeserializeDraftMove((JObject)m.Data)));
            break;

          case MessageType.ExchangeCardsMove:
            SendMoveResult(_game.MakeMove(new ExchangeCard((ArmyColor)(long)m.Data, GetCombination())));
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

    private IList<RiskCard> GetCombination()
    {
      IList<RiskCard> combination;

      combination = GetMixCombination();

      if (combination.Count == 3) return combination;

      combination = GetSameCombination();

      return combination;
    }

    private IList<RiskCard> GetMixCombination()
    {
      IList<RiskCard> combination = new List<RiskCard>();

      bool isInfatry = false;
      bool isCavalery = false;
      bool isCannon = false;
      bool isMix = false;
      foreach (var card in _cards)
      {
        if (combination.Count < 3)
        {
          switch (card.TypeUnit)
          {
            case UnitType.Infantry:
              if (!isInfatry)
              {
                combination.Add(card);
                isInfatry = true;
              }
              break;

            case UnitType.Cavalary:
              if (!isCavalery)
              {
                combination.Add(card);
                isCavalery = true;
              }
              break;

            case UnitType.Cannon:
              if (!isCannon)
              {
                combination.Add(card);
                isCavalery = true;
              }
              break;

            case UnitType.Mix:
              if (!isMix)
              {
                combination.Add(card);
                isMix = true;
              }
              break;
          }
        }
      }

      return combination;
    }

    private IList<RiskCard> GetSameCombination()
    {
      for (int i = 0; i < 4; ++i)
      {
        IList<RiskCard> combination = new List<RiskCard>();

        bool isMix = false;
        foreach (var card in _cards)
        {
          if (card.TypeUnit == (UnitType)i)
          {
            combination.Add(card);
          }
          else if (card.TypeUnit == UnitType.Mix && !isMix)
          {
            combination.Add(card);
            isMix = true;
          }
        }

        if (combination.Count == 3) return combination;
      }

      return new List<RiskCard>();
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
            SendMoveResult(_game.MakeMove(_deserializer.DeserializeFortifyMove((JObject)m.Data)));
            break;

          case MessageType.NextPhase:
            SendMoveResult(MoveResult.OK);
            isNextPhase = true;
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
            MoveResult result = _game.MakeMove(_deserializer.DeserializeSetUpMove((JObject)m.Data));
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
              if (_server.CreateGame(_deserializer.GetData<CreateGameRoomInfo>((JObject)m.Data), PlayerName))
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