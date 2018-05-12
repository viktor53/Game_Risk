using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using System.Collections.Generic;

namespace Risk.Model.GamePlan
{
  public static class GameBoardHelper
  {
    /// <summary>
    /// Checks if defender is alive.
    /// </summary>
    /// <param name="defenderColor">color of defender</param>
    /// <param name="playersInfo">information about players</param>
    /// <returns>if defender is alive</returns>
    public static bool IsDefenderAlive(ArmyColor defenderColor, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      return playersInfo[defenderColor].CapturedAreas != 0;
    }

    /// <summary>
    /// Checks if player won.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="gameBoard">game board</param>
    /// <param name="playersInfo">information about players</param>
    /// <returns>if player won</returns>
    public static bool IsWinner(ArmyColor playerColor, GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      return playersInfo[playerColor].CapturedAreas == gameBoard.Areas.Length;
    }

    /// <summary>
    /// Counts number of free units for the player.
    /// </summary>
    /// <param name="playerColor">color of player</param>
    /// <param name="gameBoard">game board</param>
    /// <returns>number of free units</returns>
    public static int GetNumberFreeUnit(ArmyColor playerColor, GameBoard gameBoard)
    {
      int occupiedAreas = 0;
      int[] areasInRegion = new int[gameBoard.ArmyForRegion.Length];

      foreach (var area in gameBoard.Areas)
      {
        if (area.ArmyColor == playerColor)
        {
          occupiedAreas++;
        }
        areasInRegion[area.RegionID]++;
      }

      int numberFreeUnits = occupiedAreas / 3;

      if (numberFreeUnits < 3)
      {
        numberFreeUnits = 3;
      }

      for (int i = 0; i < gameBoard.ArmyForRegion.Length; ++i)
      {
        if (IsControlingRegion(i, areasInRegion, playerColor, gameBoard))
        {
          numberFreeUnits += gameBoard.ArmyForRegion[i];
        }
      }

      return numberFreeUnits;
    }

    /// <summary>
    /// Finds out if the player controls the region.
    /// </summary>
    /// <param name="regionID">id of region</param>
    /// <param name="areasInRegion">number of areas in the region</param>
    /// <param name="playerColor">color of player</param>
    /// <param name="gameBoard">game board</param>
    /// <returns>if the player controls the region</returns>
    private static bool IsControlingRegion(int regionID, int[] areasInRegion, ArmyColor playerColor, GameBoard gameBoard)
    {
      int startAreaID = 0;
      int endAreaID = 0;
      for (int i = 0; i < regionID; ++i)
      {
        startAreaID += areasInRegion[i];
      }

      endAreaID = startAreaID + areasInRegion[regionID];

      for (int i = startAreaID; i < endAreaID; ++i)
      {
        if (gameBoard.Areas[i].ArmyColor != playerColor)
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Finds out if there is neutral area.
    /// </summary>
    /// <returns>if there is neutral area</returns>
    public static bool IsThereNeutralArea(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo)
    {
      int capturedAreas = 0;
      foreach (var info in playersInfo)
      {
        capturedAreas += info.Value.CapturedAreas;
      }

      return capturedAreas != gameBoard.Areas.Length;
    }

    /// <summary>
    /// Randomly sets up players order.
    /// <param name="players">players in game</param>
    /// <param name="gameBoard">game board</param>
    /// <returns>ordered players</returns>
    /// </summary>
    public static IList<IPlayer> SetUpPlayersOrder(IList<IPlayer> players, GameBoard gameBoard)
    {
      List<IPlayer> orderedPlayers = new List<IPlayer>();
      while (players.Count != 0)
      {
        Dictionary<IPlayer, int> playersRoll = new Dictionary<IPlayer, int>();
        foreach (var player in players)
        {
          playersRoll.Add(player, gameBoard.Dice.RollDice(1)[0]);
        }
        IPlayer max = GetMax(playersRoll);
        if (max != null)
        {
          orderedPlayers.Add(max);
          players.Remove(max);
        }
      }
      return orderedPlayers;
    }

    /// <summary>
    /// Gets player with maximum roll.
    /// </summary>
    /// <param name="playersRoll">players and their rolls</param>
    /// <returns>player with maximum roll or null if does not exist</returns>
    private static IPlayer GetMax(Dictionary<IPlayer, int> playersRoll)
    {
      int max = 0;
      IPlayer p = null;
      foreach (var pr in playersRoll)
      {
        if (max < pr.Value)
        {
          max = pr.Value;
          p = pr.Key;
        }
      }

      playersRoll.Remove(p);

      if (!playersRoll.ContainsValue(max))
      {
        return p;
      }
      else
      {
        return null;
      }
    }

    /// <summary>
    /// Determines if the combination of risk cards is correct.
    /// </summary>
    /// <param name="combination">risk cards combination</param>
    /// <returns>if the combination is correct</returns>
    public static bool IsCorrectCombination(IList<RiskCard> combination)
    {
      if (combination.Count == 3)
      {
        if (IsMixCom(combination) || IsComOfType(combination, UnitType.Infantry) || IsComOfType(combination, UnitType.Cannon) || IsComOfType(combination, UnitType.Cavalary))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Determines if it is risk cards combination of the specific type.
    /// (three Infantries, three Cavaleries, three Cannons or with joker)
    /// </summary>
    /// <param name="combination">risk cards combination</param>
    /// <param name="unit">unit type combination of risk cards</param>
    /// <returns>if it is risk cards combination</returns>
    private static bool IsComOfType(IList<RiskCard> combination, UnitType unit)
    {
      bool isMix = false;
      foreach (var card in combination)
      {
        if (card.TypeUnit != unit && ((isMix && card.TypeUnit == UnitType.Mix) || card.TypeUnit != UnitType.Mix))
        {
          return false;
        }
        if (card.TypeUnit == UnitType.Mix)
        {
          isMix = true;
        }
      }
      return true;
    }

    /// <summary>
    /// Determines if it is mixed risk cards combination.
    /// (one Infantry, one Cavalery, one Cannon or with joker)
    /// </summary>
    /// <param name="combination">risk cards combination</param>
    /// <returns>if it is risk cards combination</returns>
    private static bool IsMixCom(IList<RiskCard> combination)
    {
      bool isInfantry = false;
      bool isCavalery = false;
      bool isCannon = false;
      bool isMix = false;

      foreach (var card in combination)
      {
        switch (card.TypeUnit)
        {
          case UnitType.Infantry:
            isInfantry = true;
            break;

          case UnitType.Cavalary:
            isCavalery = true;
            break;

          case UnitType.Cannon:
            isCannon = true;
            break;

          case UnitType.Mix:
            isMix = true;
            break;
        }
      }

      if (isMix)
      {
        return (isInfantry && isCavalery) || (isInfantry && isCannon) || (isCavalery && isCannon);
      }
      else
      {
        return isInfantry && isCavalery && isCannon;
      }
    }

    /// <summary>
    /// Determines if two areas are connected through friendly areas.
    /// </summary>
    /// <param name="fromAreaID">id of firt area</param>
    /// <param name="toAreaID">id of second area</param>
    /// <param name="gameBoard">game board</param>
    /// <returns></returns>
    public static bool IsConnected(int fromAreaID, int toAreaID, GameBoard gameBoard)
    {
      ArmyColor fromColor = gameBoard.Areas[fromAreaID].ArmyColor;
      if (fromColor == gameBoard.Areas[toAreaID].ArmyColor)
      {
        Queue<int> toVisit = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        toVisit.Enqueue(fromAreaID);

        while (toVisit.Count != 0)
        {
          int areaID = toVisit.Dequeue();

          if (areaID == toAreaID)
          {
            return true;
          }

          visited.Add(areaID);

          for (int i = 0; i < gameBoard.Connections[areaID].Length; ++i)
          {
            if (gameBoard.Connections[areaID][i] && gameBoard.Areas[i].ArmyColor == fromColor && !visited.Contains(i))
            {
              toVisit.Enqueue(i);
            }
          }
        }
      }

      return false;
    }
  }
}