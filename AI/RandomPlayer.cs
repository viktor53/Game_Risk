using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using Risk.Model.GameCore.Moves;
using System.Threading;

namespace Risk.AI
{
  /// <summary>
  /// Player who makes random moves.
  /// </summary>
  public class RandomPlayer : IAI
  {
    private int _freeUnit;

    public ArmyColor _aiColor;

    private List<RiskCard> _cardsInHand;

    private IGame _game;

    private GamePlanInfo _gamePlan;

    private object _gamePlanLock;

    private bool _isWinner;

    private Random _ran;

    public int FreeUnit
    {
      get
      {
        return _freeUnit;
      }

      set
      {
        _freeUnit = value;
      }
    }

    public ArmyColor PlayerColor
    {
      get
      {
        return _aiColor;
      }

      set
      {
        _aiColor = value;
      }
    }

    public bool IsWinner => _isWinner;

    public void AddCard(RiskCard card)
    {
      _cardsInHand.Add(card);
    }

    public void RemoveCard(RiskCard card)
    {
      _cardsInHand.Remove(card);
    }

    /// <summary>
    /// Probability of attacking.
    /// </summary>
    public double Probability { get; set; }

    public double GetAvgTimeSetUp => time[0] / count[0];

    public double GetAvgTimeDraft => time[1] / count[1];

    public double GetAvgTimeAttack => time[2] / count[2];

    public double GetAvgTimeFortify => time[3] / count[3];

    private Stopwatch _sw = new Stopwatch();

    private double[] time = new double[] { 0, 0, 0, 0 };

    private int[] count = new int[] { 0, 0, 0, 0 };

    public RandomPlayer(ArmyColor playerColor)
    {
      _aiColor = playerColor;
      _ran = new Random();
      _gamePlanLock = new object();
      Probability = 87;
      _cardsInHand = new List<RiskCard>();
    }

    public async Task StartPlayer(IGame game)
    {
      await Task.Run(() =>
      {
        _game = game;
        _gamePlan = game.GetGamePlan();
      });
    }

    public void PlayAttack()
    {
      _sw.Restart();

      IList<Area> canAttack = Helper.WhoCanAttack(_gamePlan.Areas, _gamePlan.Connections, _aiColor);

      int probibility = _ran.Next(100);
      while (probibility >= (100 - Probability) && canAttack.Count > 0)
      {
        int attacker = _ran.Next(canAttack.Count);
        IList<Area> canBeAttacked = Helper.WhoCanBeAttacked(canAttack[attacker], _gamePlan.Areas, _gamePlan.Connections, _aiColor);

        int defender = _ran.Next(canBeAttacked.Count);
        int attackSize = _ran.Next(1, Helper.GetMaxSizeOfAttack(canAttack[attacker]) + 1);

        MoveResult result = _game.MakeMove(new Attack(_aiColor, canAttack[attacker].ID, canBeAttacked[defender].ID, (AttackSize)attackSize));

        if (result == MoveResult.AreaCaptured)
        {
          _game.MakeMove(new Capture(_aiColor, _ran.Next(attackSize, canAttack[attacker].SizeOfArmy)));

          for (int i = 0; i < _gamePlan.Connections[canBeAttacked[defender].ID].Length; ++i)
          {
            if (_gamePlan.Connections[canBeAttacked[defender].ID][i] && _gamePlan.Areas[i].ArmyColor == _aiColor)
            {
              if (canAttack.Contains(_gamePlan.Areas[i]) && !Helper.CanAttack(_gamePlan.Areas[i], _gamePlan.Areas, _gamePlan.Connections, _aiColor))
              {
                canAttack.Remove(_gamePlan.Areas[i]);
              }
            }
          }

          if (Helper.CanAttack(canBeAttacked[defender], _gamePlan.Areas, _gamePlan.Connections, _aiColor))
          {
            canAttack.Add(canBeAttacked[defender]);
          }

          canBeAttacked.Remove(canBeAttacked[defender]);
        }
        else
        {
          if (canAttack[attacker].SizeOfArmy == 1)
          {
            canAttack.Remove(canAttack[attacker]);
          }
        }

        if (result == MoveResult.Winner)
        {
          return;
        }

        probibility = _ran.Next(100);
      }

      _sw.Stop();
      time[2] += _sw.Elapsed.TotalSeconds;
      count[2]++;
    }

    public void PlayDraft()
    {
      _sw.Restart();

      while (_cardsInHand.Count >= 5)
      {
        IList<RiskCard> combination = Helper.GetCombination(_cardsInHand);
        MoveResult result = _game.MakeMove(new ExchangeCard(_aiColor, combination));
      }

      if (_cardsInHand.Count >= 3)
      {
        int probibility = _ran.Next(100);
        if (probibility >= 50)
        {
          IList<RiskCard> combination = Helper.GetCombination(_cardsInHand);
          if (combination.Count == 3)
          {
            MoveResult result = _game.MakeMove(new ExchangeCard(_aiColor, combination));
          }
        }
      }

      IList<Area> posibilities = Helper.GetMyAreas(_gamePlan.Areas, _aiColor);

      while (FreeUnit > 0)
      {
        int result = _ran.Next(posibilities.Count);
        int numberOfarmies = _ran.Next(FreeUnit + 1);
        MoveResult r = _game.MakeMove(new Draft(_aiColor, posibilities[result].ID, numberOfarmies));
      }

      _sw.Stop();
      time[1] += _sw.Elapsed.TotalSeconds;
      count[1]++;
    }

    public void PlayFortify()
    {
      _sw.Restart();

      int probibility = _ran.Next(100);
      if (probibility >= 50)
      {
        IList<Area> canFortify = Helper.WhoCanFortify(_gamePlan.Areas, _gamePlan.Connections, _aiColor);
        if (canFortify.Count > 0)
        {
          int from = _ran.Next(canFortify.Count);
          IList<Area> where = Helper.WhereCanFortify(canFortify[from], _gamePlan.Areas, _gamePlan.Connections, _aiColor);

          int to = _ran.Next(where.Count);
          int sizeOfArmy = _ran.Next(1, canFortify[from].SizeOfArmy);
          MoveResult result = _game.MakeMove(new Fortify(_aiColor, canFortify[from].ID, where[to].ID, sizeOfArmy));
        }
      }

      _sw.Stop();
      time[3] += _sw.Elapsed.TotalSeconds;
      count[3]++;
    }

    public void PlaySetUp()
    {
      _sw.Restart();

      while (_gamePlan == null)
      {
        Thread.Sleep(5);
      }

      IList<Area> possibilities;

      possibilities = Helper.GetUnoccupiedAreas(_gamePlan.Areas);

      if (possibilities.Count == 0)
      {
        possibilities = Helper.GetMyAreas(_gamePlan.Areas, PlayerColor);
      }

      int result = _ran.Next(possibilities.Count);

      MoveResult r = _game.MakeMove(new SetUp(_aiColor, possibilities[result].ID));

      _sw.Stop();
      time[0] += _sw.Elapsed.TotalSeconds;
      count[0]++;
    }

    public async Task EndPlayer(bool isWinner)
    {
      await Task.Run(() =>
      {
        _isWinner = isWinner;
        _cardsInHand.Clear();
      });
    }

    public async Task UpdateGame(byte areaID, ArmyColor armyColor, int sizeOfArmy)
    {
      await Task.Run(() =>
      {
        _gamePlan.Areas[areaID].ArmyColor = armyColor;
        _gamePlan.Areas[areaID].SizeOfArmy = sizeOfArmy;
      });
    }
  }
}