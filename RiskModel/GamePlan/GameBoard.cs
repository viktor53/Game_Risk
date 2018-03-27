using System;
using System.Collections.Generic;
using Risk.Model.Cards;
using Risk.Model.Enums;

namespace Risk.Model.GamePlan
{
  /// <summary>
  /// Represents game board with areas, connections, dice and package of cards.
  /// </summary>
  public sealed class GameBoard
  {
    private Queue<RiskCard> _package;

    private IList<RiskCard> _returnedCards;

    private int _combination;

    private int _unitsPerCombination;

    /// <summary>
    /// Connections between areas.
    /// </summary>
    public bool[][] Connections { get; private set; }

    /// <summary>
    /// Areas of game plane.
    /// </summary>
    public Area[] Areas { get; private set; }

    /// <summary>
    /// Dice providing rolls.
    /// </summary>
    public Dice Dice { get; private set; }

    /// <summary>
    /// Determines free units for occupied region with the ID.
    /// </summary>
    public int[] ArmyForRegion { get; private set; }

    /// <summary>
    /// Initializes connections and areas, but does not create game plan.
    /// </summary>
    /// <param name="countOfAreas">number of areas</param>
    /// <param name="armyForRegion">free unit for occupied region</param>
    /// <param name="package">package of risk cards</param>
    public GameBoard(int countOfAreas, int[] armyForRegion, IList<RiskCard> package)
    {
      ArmyForRegion = armyForRegion;

      Connections = new bool[countOfAreas][];
      for (int i = 0; i < countOfAreas; i++)
      {
        Connections[i] = new bool[countOfAreas];
      }

      Areas = new Area[countOfAreas];

      _package = new Queue<RiskCard>();
      _returnedCards = package;

      ShuffleCards();
      PutInThePackage();

      Dice = new Dice();

      _combination = 1;
      _unitsPerCombination = 4;
    }

    /// <summary>
    /// Gets risk card from top of package.
    /// </summary>
    /// <returns>risk card from top</returns>
    public RiskCard GetCard()
    {
      if (_package.Count == 0)
      {
        ShuffleCards();
        PutInThePackage();
      }
      return _package.Dequeue();
    }

    /// <summary>
    /// Returns cards into package.
    /// </summary>
    /// <param name="card">returned risk card</param>
    public void ReturnCard(RiskCard card)
    {
      _returnedCards.Add(card);
    }

    /// <summary>
    /// Gets free units for exchange risk cards combination.
    /// </summary>
    /// <returns>free units</returns>
    public int GetUnitPerCombination()
    {
      if (_unitsPerCombination == 50)
      {
        int units = _unitsPerCombination;
        switch (_combination)
        {
          case 1:
          case 2:
          case 3:
          case 4:
            _unitsPerCombination += 2;
            break;

          case 5:
            _unitsPerCombination += 3;
            break;

          default:
            _unitsPerCombination += 5;
            break;
        }

        _combination++;

        return units;
      }
      return _unitsPerCombination;
    }

    /// <summary>
    /// Determines if the combination of risk cards is correct.
    /// </summary>
    /// <param name="combination">risk cards combination</param>
    /// <returns>if the combination is correct</returns>
    public bool IsCorrectCombination(IList<RiskCard> combination)
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
    private bool IsComOfType(IList<RiskCard> combination, UnitType unit)
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
    private bool IsMixCom(IList<RiskCard> combination)
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
    /// <returns></returns>
    public bool IsConnected(int fromAreaID, int toAreaID)
    {
      ArmyColor fromColor = Areas[fromAreaID].ArmyColor;
      if (fromColor == Areas[toAreaID].ArmyColor)
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

          for (int i = 0; i < Connections[areaID].Length; ++i)
          {
            if (Connections[areaID][i] && Areas[i].ArmyColor == fromColor && !visited.Contains(i))
            {
              toVisit.Enqueue(i);
            }
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Shuffles returned cards.
    /// </summary>
    private void ShuffleCards()
    {
      Random ran = new Random();
      for (int i = 0; i < _returnedCards.Count - 1; ++i)
      {
        int j = ran.Next(i, _returnedCards.Count);
        RiskCard tmp = _returnedCards[i];
        _returnedCards[i] = _returnedCards[j];
        _returnedCards[j] = tmp;
      }
    }

    /// <summary>
    /// Puts returned cards into package.
    /// </summary>
    private void PutInThePackage()
    {
      foreach (var card in _returnedCards)
      {
        _package.Enqueue(card);
      }
      _returnedCards.Clear();
    }
  }
}