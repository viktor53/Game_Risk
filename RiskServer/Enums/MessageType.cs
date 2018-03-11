using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking.Enums
{
  public enum MessageType
  {
    Confirmation = 0,
    Error = 1,
    Registration = 2,
    Logout = 3,
    ConnectToGame = 4,
    CreateGame = 5,
    UpdateGameList = 6,
    UpdatePlayerListAdd = 7,
    UpdatePlayerListRemove = 8,
    ReadyTag = 9,
    InitializeGame = 10,
    SetUpMove = 11,
    DraftMove = 12,
    ExchangeCardsMove = 13,
    AttackMove = 14,
    CaptureMove = 15,
    FortifyMove = 16,
    NextPhase = 17,
    MoveResult = 18,
    YourTurn = 19,
    UpdateGame = 20,
    EndGame = 21,
    Leave = 22,
    FreeUnit = 23,
    ArmyColor = 24
  }
}