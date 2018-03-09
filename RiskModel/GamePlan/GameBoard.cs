using System.Collections.Generic;
using Risk.Model.Cards;
using System;
using Risk.Model.Enums;

namespace Risk.Model.GamePlan
{
  public sealed class GameBoard
  {
    public bool[][] Connections { get; private set; }

    public Area[] Areas { get; private set; }

    public Dice Dice { get; private set; }

    public int[] ArmyForRegion { get; private set; }

    private Queue<RiskCard> _package;

    private IList<RiskCard> _returnedCards;

    private int _combination;

    private int _unitsPerCombination;

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

    public RiskCard GetCard()
    {
      if (_package.Count == 0)
      {
        ShuffleCards();
        PutInThePackage();
      }
      return _package.Dequeue();
    }

    public void ReturnCard(RiskCard card)
    {
      _returnedCards.Add(card);
    }

    public int GetUnitPerCombination()
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

    private bool IsComOfType(IList<RiskCard> combination, UnitType unit)
    {
      foreach (var card in combination)
      {
        if (card.TypeUnit != unit || card.TypeUnit != UnitType.Mix)
        {
          return false;
        }
      }
      return true;
    }

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