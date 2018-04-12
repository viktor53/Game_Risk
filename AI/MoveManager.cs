using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.Cards;

namespace Risk.AI
{
  internal static class MoveManager
  {
    public static void MakeMove(SetUp move, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      if (gameBoard.Areas[move.AreaID].ArmyColor != move.PlayerColor)
      {
        playersInfo[move.PlayerColor].CapturedAreas++;
      }

      gameBoard.Areas[move.AreaID].SizeOfArmy++;
      gameBoard.Areas[move.AreaID].ArmyColor = move.PlayerColor;
      playersInfo[move.PlayerColor].FreeUnits--;
    }

    public static void MakeMove(Draft move, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      playersInfo[move.PlayerColor].GetsCard = false;

      gameBoard.Areas[move.AreaID].SizeOfArmy += move.NumberOfUnit;
      playersInfo[move.PlayerColor].FreeUnits -= move.NumberOfUnit;
    }

    public static void MakeMove(ExchangeCard move, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      int units = gameBoard.GetUnitPerCombination();
      playersInfo[move.PlayerColor].FreeUnits += units;

      //foreach (var card in move.Combination)
      //{
      //  if (card.TypeUnit != UnitType.Mix)
      //  {
      //    int id = ((NormalCard)card).Area;
      //    if (gameBoard.Areas[id].ArmyColor == move.PlayerColor)
      //    {
      //      gameBoard.Areas[id].SizeOfArmy += 2;

      //      break;
      //    }
      //  }
      //}

      RemoveCards(move, gameBoard, playersInfo);
    }

    private static void RemoveCards(ExchangeCard move, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      foreach (var card in move.Combination)
      {
        gameBoard.ReturnCard(card);
        playersInfo[move.PlayerColor].Cards.Remove(card);
      }
    }

    public static MoveResult MakeMove(Attack move, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      CountLosts(move, gameBoard);

      return FindOutResult(move, gameBoard, playersInfo);
    }

    private static void CountLosts(Attack move, GameBoard gameBoard)
    {
      int[] attRoll = gameBoard.Dice.RollDice((int)move.AttackSize);
      int[] defRoll = gameBoard.Areas[move.DefenderAreaID].SizeOfArmy >= 2 ? gameBoard.Dice.RollDice(2) : gameBoard.Dice.RollDice(1);

      int attDied = 0;
      int defDied = 0;
      for (int i = 0; i < Math.Min(attRoll.Length, defRoll.Length); ++i)
      {
        if (attRoll[i] > defRoll[i])
        {
          defDied++;
        }
        else
        {
          attDied++;
        }
      }

      gameBoard.Areas[move.AttackerAreaID].SizeOfArmy -= attDied;
      gameBoard.Areas[move.DefenderAreaID].SizeOfArmy -= defDied;
    }

    private static MoveResult FindOutResult(Attack move, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      if (gameBoard.Areas[move.DefenderAreaID].SizeOfArmy == 0)
      {
        ArmyColor defColor = gameBoard.Areas[move.DefenderAreaID].ArmyColor;

        playersInfo[move.PlayerColor].CapturedAreas++;
        playersInfo[defColor].CapturedAreas--;

        gameBoard.Areas[move.DefenderAreaID].ArmyColor = move.PlayerColor;

        playersInfo[defColor].IsAlive = GameBoardHelper.IsDefenderAlive(defColor, playersInfo);

        if (!playersInfo[defColor].IsAlive)
        {
          foreach (var card in playersInfo[defColor].Cards)
          {
            playersInfo[move.PlayerColor].Cards.Add(card);
          }

          playersInfo[defColor].Cards.Clear();
        }

        if (GameBoardHelper.IsWinner(move.PlayerColor, gameBoard, playersInfo))
        {
          return MoveResult.Winner;
        }
        else
        {
          return MoveResult.AreaCaptured;
        }
      }

      return MoveResult.OK;
    }

    public static void MakeMove(Capture move, Attack capturing, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      gameBoard.Areas[capturing.AttackerAreaID].SizeOfArmy -= move.ArmyToMove;
      gameBoard.Areas[capturing.DefenderAreaID].SizeOfArmy += move.ArmyToMove;

      if (!playersInfo[move.PlayerColor].GetsCard)
      {
        RiskCard card = gameBoard.GetCard();
        playersInfo[move.PlayerColor].Cards.Add(card);

        playersInfo[move.PlayerColor].GetsCard = true;
      }
    }

    public static void MakeMove(Fortify move, GameBoard gameBoard)
    {
      gameBoard.Areas[move.FromAreaID].SizeOfArmy -= move.SizeOfArmy;
      gameBoard.Areas[move.ToAreaID].SizeOfArmy += move.SizeOfArmy;
    }
  }
}