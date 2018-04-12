using System;
using System.Collections.Generic;
using Risk.Model.Cards;
using Risk.Model.Enums;

namespace Risk.Model.GamePlan
{
  /// <summary>
  /// Represents game board with areas, connections, dice and package of cards.
  /// </summary>
  public sealed class GameBoard : ICloneable
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
    /// Copy constructor
    /// </summary>
    /// <param name="gameBoard">original game board</param>
    private GameBoard(GameBoard gameBoard)
    {
      // deep copy game plan
      Connections = gameBoard.Connections;
      Areas = new Area[gameBoard.Areas.Length];

      for (int i = 0; i < Areas.Length; ++i)
      {
        Areas[i] = (Area)gameBoard.Areas[i].Clone();
      }

      Dice = new Dice();
      ArmyForRegion = gameBoard.ArmyForRegion;

      // copy combination information
      _combination = gameBoard._combination;
      _unitsPerCombination = gameBoard._unitsPerCombination;

      // shallow copy cards state
      var package = new List<RiskCard>(gameBoard._package);
      _package = new Queue<RiskCard>();
      _returnedCards = package;
      PutInThePackage();
      package.AddRange(gameBoard._returnedCards);
    }

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

    public object Clone()
    {
      return new GameBoard(this);
    }
  }
}