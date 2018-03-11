using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.Enums
{
  public enum MoveResult
  {
    OK,
    BadPhase,
    NotYourTurn,
    AlreadySetUpThisTurn,
    AlreadyFortifyThisTurn,
    NotEnoughFreeUnit,
    NotYourArea,
    InvalidAttackerOrDefender,
    InvalidAttack,
    AreaCaptured,
    EmptyCapturedArea,
    NoCapturedArea,
    InvalidNumberUnit,
    NotConnected,
    Winner,
    InvalidCombination
  }
}