using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Risk.Networking.Messages;
using Risk.Networking.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Networking.Messages.Data;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;

namespace Risk.Networking.Server
{
  /// <summary>
  /// Represents connection with the player on client side. Implements IClientManager.
  /// </summary>
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

    private object _listeningLock;

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

    /// <summary>
    /// Creates player connected player.
    /// </summary>
    /// <param name="connection">connection with the client</param>
    /// <param name="server">risk server</param>
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

    /// <summary>
    /// Adds card into player's hand and notifies him.
    /// </summary>
    /// <param name="card">risk card for player</param>
    public void AddCard(RiskCard card)
    {
      _cards.Add(card);
      Task.Run(() =>
      {
        SendMessage(new Message(MessageType.UpdateCard, true));
      });
    }

    /// <summary>
    /// Removes card from player's hand and notifies him.
    /// </summary>
    /// <param name="card">risk card from its hand</param>
    public void RemoveCard(RiskCard card)
    {
      _cards.Remove(card);
      Task.Run(() =>
      {
        SendMessage(new Message(MessageType.UpdateCard, false));
      });
    }

    /// <summary>
    /// Plays attack phase.
    /// </summary>
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

    /// <summary>
    /// Plays draft phase.
    /// </summary>
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

    /// <summary>
    /// Gets risk cards combination if it exists.
    /// </summary>
    /// <returns>risk cards combination or empty combination</returns>
    private IList<RiskCard> GetCombination()
    {
      IList<RiskCard> combination;

      combination = GetMixCombination();

      if (combination.Count == 3) return combination;

      combination = GetSameCombination();

      return combination;
    }

    /// <summary>
    /// Gets mix risk cards combination.
    /// </summary>
    /// <returns>mix risk cards combination or not full combination</returns>
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

    /// <summary>
    /// Gets same risk cards combination.
    /// </summary>
    /// <returns>same risk cards combination or empty combination</returns>
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

    /// <summary>
    /// Plays fortify phase.
    /// </summary>
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

    /// <summary>
    /// Plays setup phase.
    /// </summary>
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

    /// <summary>
    /// Asynchronously updates player's game board.
    /// </summary>
    /// <param name="area">changed area</param>
    /// <returns></returns>
    public async Task UpdateGame(byte areaID, ArmyColor armyColor, int sizeOfArmy)
    {
      await Task.Run(() =>
      {
        Area updated = new Area(areaID, 0);
        updated.ArmyColor = armyColor;
        updated.SizeOfArmy = sizeOfArmy;
        Message m = new Message(MessageType.UpdateGame, updated);
        SendMessage(m);
      });
    }

    /// <summary>
    /// Sends to player that it is his turn.
    /// </summary>
    /// <param name="isSetUp">notify if it is setup or draft phase</param>
    private void SendYourTurnMessage(bool isSetUp)
    {
      Message m = new Message(MessageType.YourTurn, isSetUp);
      SendMessage(m);
    }

    /// <summary>
    /// Sends to player result of his move.
    /// </summary>
    /// <param name="result">result of player's move</param>
    private void SendMoveResult(MoveResult result)
    {
      Message m = new Message(MessageType.MoveResult, result);
      SendMessage(m);
    }

    /// <summary>
    /// Sends to player error message that its request is bad or unknown
    /// </summary>
    private void SendErrorMessage()
    {
      Message m = new Message(MessageType.Error, new Error(ErrorType.UknownRequest, "Unknown or bad request!"));
      SendMessage(m);
    }

    /// <summary>
    /// Sends message to player.
    /// </summary>
    /// <param name="m">message for player</param>
    private void SendMessage(Message m)
    {
      lock (_sendingLock)
      {
        _connection.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m)));
        Thread.Sleep(100);
      }
    }

    /// <summary>
    /// Receives message from player.
    /// </summary>
    /// <returns>received message</returns>
    private Message ReceiveMessage()
    {
      int lengthOfData = _connection.Receive(_buffer);
      return JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengthOfData));
    }

    /// <summary>
    /// Asynchronously starts player and sends inicialization of game board.
    /// </summary>
    /// <param name="game">started game</param>
    public async Task StartPlayer(IGame game)
    {
      _game = game;
      Message m = new Message(MessageType.InitializeGame, GameRoom.GetBoardInfo(_game.GetGamePlan()));
      await Task.Run(() =>
      {
        SendMessage(m);

        SendMessage(new Message(MessageType.ArmyColor, PlayerColor));
      });
    }

    /// <summary>
    /// Asynchronously ends player and notifies if is winner.
    /// </summary>
    /// <param name="isWinner">if player is winner</param>
    public async Task EndPlayer(bool isWinner)
    {
      await Task.Run(() =>
      {
        Message m = new Message(MessageType.EndGame, isWinner);
        SendMessage(m);
        ManagingConnectingToRoom();
      });
    }

    /// <summary>
    /// Asynchronously sends name of new connected player.
    /// </summary>
    /// <param name="name">name of new connected player</param>
    /// <returns>async task</returns>
    public async Task SendNewPlayerConnected(string name)
    {
      await Task.Run(() =>
      {
        List<string> players = new List<string>();
        players.Add(name);
        Message m = new Message(MessageType.UpdatePlayerListAdd, players);
        SendMessage(m);
      });
    }

    /// <summary>
    /// Asynchronously sends name of player that left the game room.
    /// </summary>
    /// <param name="name">name of player that left the game room</param>
    /// <returns>async task</returns>
    public async Task SendPlayerLeave(string name)
    {
      await Task.Run(() =>
      {
        List<string> players = new List<string>();
        players.Add(name);
        Message m = new Message(MessageType.UpdatePlayerListRemove, players);
        SendMessage(m);
      });
    }

    /// <summary>
    /// Asynchronously sends names of connected players.
    /// </summary>
    /// <param name="names">names of connected players</param>
    /// <returns>async task</returns>
    public async Task SendConnectedPlayers(IList<string> names)
    {
      await Task.Run(() =>
      {
        Message m = new Message(MessageType.UpdatePlayerListAdd, names);
        SendMessage(m);
      });
    }

    /// <summary>
    /// Asynchronously listens to ready tag or leaving notification from client.
    /// </summary>
    /// <returns>async task</returns>
    public async Task ListenToReadyTag()
    {
      if (!_listenToReady)
      {
        await Task.Run(() =>
        {
          try
          {
            _listenToReady = true;
            while (_listenToReady)
            {
              Message m = ReceiveMessage();

              switch (m.MessageType)
              {
                case MessageType.ReadyTag:
                  OnReady?.Invoke(this, new EventArgs());
                  _listenToReady = false;
                  break;

                case MessageType.Leave:
                  OnLeave?.Invoke(this, new EventArgs());
                  ManagingConnectingToRoom();
                  _listenToReady = false;
                  break;

                default:
                  SendErrorMessage();
                  break;
              }
            }
          }
          catch (SocketException)
          {
            OnLeave?.Invoke(this, new EventArgs());
            _server.LogOutPlayer(PlayerName);
            return;
          }
        });
      }
    }

    /// <summary>
    /// Asynchronously listens to client's commands before connecting to game room.
    /// </summary>
    /// <returns>async task</returns>
    public async Task SartListening()
    {
      await Task.Run(() =>
      {
        try
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

          ManagingConnectingToRoom();
        }
        catch (SocketException)
        {
          if (PlayerName != null)
          {
            _server.LogOutPlayer(PlayerName);
          }
          return;
        }
      });
    }

    /// <summary>
    /// Asynchronously listens to command from client before connecting to game room.
    /// </summary>
    private async void ManagingConnectingToRoom()
    {
      await Task.Run(() =>
      {
        try
        {
          SendUpdateGameList(_server.GetUpdateInfo());

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
        }
        catch (SocketException)
        {
          _server.LogOutPlayer(PlayerName);
        }
      });
    }

    /// <summary>
    /// Asynchronously sends update of game room information list.
    /// </summary>
    /// <param name="roomsInfo">game room information list</param>
    /// <returns>async task</returns>
    public async Task SendUpdateGameList(IList<GameRoomInfo> roomsInfo)
    {
      await Task.Run(() =>
      {
        Message m = new Message(MessageType.UpdateGameList, roomsInfo);
        SendMessage(m);
      });
    }
  }
}