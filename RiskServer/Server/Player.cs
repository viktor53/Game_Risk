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

namespace Risk.Networking.Server
{
  internal class Player : IPlayer
  {
    private Socket _connection;

    private const int _bufferSize = 1024;

    private byte[] _buffer;

    private JsonSerializer _serializer;

    private Game _game;

    public IList<RiskCard> Cards { get; private set; }

    public int FreeUnit { get; set; }

    public ArmyColor PlayerColor { get; private set; }

    public Player(Socket connection, ArmyColor playerColor, Game game)
    {
      _connection = connection;
      PlayerColor = playerColor;
      _buffer = new byte[_bufferSize];
      _serializer = new JsonSerializer();
      _game = game;
    }

    public void PlayAttack()
    {
      bool isNextPhase = false;
      while (!isNextPhase)
      {
        int lengthOfData = _connection.Receive(_buffer);
        Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengthOfData));
        switch (m.MessageType)
        {
          case MessageType.AttackMove:
            SendMoveResult(_game.MakeMove(GetMove<Attack>((JObject)m.Data)));
            break;

          case MessageType.CaptureMove:
            SendMoveResult(_game.MakeMove(GetMove<Capture>((JObject)m.Data)));
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
      bool isNextPhase = false;
      while (!isNextPhase)
      {
        int lengthOfData = _connection.Receive(_buffer);
        Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengthOfData));
        switch (m.MessageType)
        {
          case MessageType.DraftMove:
            SendMoveResult(_game.MakeMove(GetMove<Draft>((JObject)m.Data)));
            break;

          case MessageType.ExchangeCardsMove:
            SendMoveResult(_game.MakeMove(GetMove<ExchangeCard>((JObject)m.Data)));
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
        int lengthOfData = _connection.Receive(_buffer);
        Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengthOfData));
        switch (m.MessageType)
        {
          case MessageType.FortifyMove:
            SendMoveResult(_game.MakeMove(GetMove<Fortify>((JObject)m.Data)));
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
        int lengthOfData = _connection.Receive(_buffer);
        Message m = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(_buffer, 0, lengthOfData));
        switch (m.MessageType)
        {
          case MessageType.SetUpMove:
            MoveResult result = _game.MakeMove(GetMove<SetUp>((JObject)m.Data));
            SendMoveResult(result);
            isCorrect = result == MoveResult.OK ? true : false;
            break;

          default:
            SendErrorMessage();
            break;
        }
      }
    }

    private T GetMove<T>(JObject data)
    {
      using (JTokenReader reader = new JTokenReader(data))
      {
        return _serializer.Deserialize<T>(reader);
      }
    }

    private void SendYourTurnMessage()
    {
      Message m = new Message(MessageType.YourTurn, null);
      _connection.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m)));
    }

    private void SendMoveResult(MoveResult result)
    {
      Message m = new Message(MessageType.MoveResult, result);
      _connection.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m)));
    }

    private void SendErrorMessage()
    {
      Message m = new Message(MessageType.Error, null);
      _connection.Send(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(m)));
    }
  }
}