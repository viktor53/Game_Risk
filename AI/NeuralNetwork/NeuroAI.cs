using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using Risk.Model.GameCore.Moves;
using System.Diagnostics;

namespace Risk.AI.NeuralNetwork
{
  public class NeuroAI : IAI
  {
    private ActivationNetwork _setUpNetwork;

    private ActivationNetwork _draftNetwork;

    private ActivationNetwork _exchangeCardNetwork;

    private ActivationNetwork _attackNetwork;

    private ActivationNetwork _fortifyNetwork;

    private ArmyColor _aiColor;

    private int _freeUnit;

    private IList<RiskCard> _cardsInHand;

    private IGame _game;

    private GamePlanInfo _gamePlan;

    private IDictionary<int, RegionInformation> _regionsInfo;

    private bool _isWinner;

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

    public void AddCard(RiskCard card)
    {
      _cardsInHand.Add(card);
    }

    public void RemoveCard(RiskCard card)
    {
      _cardsInHand.Remove(card);
    }

    public bool IsWinner => _isWinner;

    public double GetAvgTimeSetUp => time[0] / count[0];

    public double GetAvgTimeDraft => time[1] / count[1];

    public double GetAvgTimeAttack => time[2] / count[2];

    public double GetAvgTimeFortify => time[3] / count[3];

    public NeuroAI(ArmyColor aiColor, ActivationNetwork setUpNetwork, ActivationNetwork draftNetwork, ActivationNetwork exchangeNetwork,
      ActivationNetwork attackNetwork, ActivationNetwork fortifyNetwork)
    {
      _aiColor = aiColor;
      _setUpNetwork = setUpNetwork;
      _draftNetwork = draftNetwork;
      _exchangeCardNetwork = exchangeNetwork;
      _attackNetwork = attackNetwork;
      _fortifyNetwork = fortifyNetwork;
      _cardsInHand = new List<RiskCard>();
    }

    public async Task StartPlayer(IGame game)
    {
      await Task.Run(() =>
      {
        _game = game;
        _gamePlan = game.GetGamePlan();
        _regionsInfo = NeuroHelper.GetRegionsInformation(_gamePlan.Areas, _gamePlan.Connections, game.GetBonusForRegions());
      });
    }

    public async Task EndPlayer(bool isWinner)
    {
      await Task.Run(() =>
      {
        _isWinner = isWinner;
        _cardsInHand.Clear();
      });
    }

    private Stopwatch _sw = new Stopwatch();

    private double[] time = new double[] { 0, 0, 0, 0 };

    private int[] count = new int[] { 0, 0, 0, 0 };

    public void PlaySetUp()
    {
      _sw.Restart();

      double[] input = new double[16];

      IList<Area> possibilities = Helper.GetUnoccupiedAreas(_gamePlan.Areas);

      if (possibilities.Count == 0)
      {
        possibilities = Helper.GetMyAreas(_gamePlan.Areas, _aiColor);
      }

      double max = 0;
      int best = 0;

      foreach (var area in possibilities)
      {
        PrepareSetUpInput(area, input);

        double result = _setUpNetwork.Compute(input)[0];

        if (result > max)
        {
          max = result;
          best = area.ID;
        }
      }

      MoveResult mr = _game.MakeMove(new SetUp(_aiColor, best));

      bool bad = mr != MoveResult.OK;

      _sw.Stop();
      time[0] += _sw.Elapsed.TotalSeconds;
      count[0]++;
    }

    private void PrepareSetUpInput(Area area, double[] input)
    {
      const int levels = 2;

      if (area.ArmyColor == ArmyColor.Neutral)
      {
        input[0] = 0;
      }
      else
      {
        input[0] = 1;
      }

      AddSurroundingsInfoToInput(area, levels, 1, input);

      int offset = levels * 4;

      input[offset] = _regionsInfo[area.RegionID].NumberOfAreas;
      offset++;

      Tuple<int, int> stateOfRegion = NeuroHelper.GetRegionState(_gamePlan.Areas, area.RegionID, _regionsInfo, _aiColor);

      input[offset] = stateOfRegion.Item1;
      offset++;
      input[offset] = stateOfRegion.Item2;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].BonusForRegion;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].NumberOfAreasForOneArmy;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].NumberOfDefendArmies;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].DefendRate;
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
        IList<RiskCard> combination = Helper.GetCombination(_cardsInHand);
        if (combination.Count == 3)
        {
          double[] inputEx = new double[4];

          PrepareExchangeCardInput(inputEx);

          double result = _exchangeCardNetwork.Compute(inputEx)[0];

          if (result > 0.5)
          {
            _game.MakeMove(new ExchangeCard(_aiColor, combination));
          }
        }
      }

      IList<Area> possibilities = Helper.GetMyAreas(_gamePlan.Areas, _aiColor);

      double[] input = new double[9];

      while (FreeUnit > 0)
      {
        double max = 0;
        double army = 0;
        int area = 0;

        for (int i = 0; i < possibilities.Count; ++i)
        {
          PrepareDraftInput(possibilities[i], input);

          double[] result = _draftNetwork.Compute(input);

          if (result[0] > max)
          {
            max = result[0];
            army = result[1];
            area = possibilities[i].ID;
          }
        }

        int resultArmy = (int)Math.Round(army * FreeUnit);
        if (resultArmy == 0)
        {
          resultArmy = 1;
        }

        MoveResult mr = _game.MakeMove(new Draft(_aiColor, area, resultArmy));
        bool bad = mr != MoveResult.OK;
      }

      _sw.Stop();
      time[1] += _sw.Elapsed.TotalSeconds;
      count[1]++;
    }

    private void PrepareExchangeCardInput(double[] input)
    {
      input[0] = FreeUnit;
      input[1] = _game.GetNumberOfCombination();

      Tuple<int, int> state = NeuroHelper.GetStateOfBorders(_gamePlan.Areas, _gamePlan.Connections, _aiColor);

      input[2] = state.Item1;
      input[3] = state.Item2;
    }

    private void PrepareDraftInput(Area area, double[] input)
    {
      const int levels = 2;

      input[0] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(area, levels, 1, input);
    }

    public void PlayAttack()
    {
      _sw.Restart();

      IList<Area> possibilities = Helper.WhoCanAttack(_gamePlan.Areas, _gamePlan.Connections, _aiColor);

      double[] input = new double[17];

      for (int i = 0; i < possibilities.Count; ++i)
      {
        IList<Area> target = Helper.WhoCanBeAttacked(possibilities[i], _gamePlan.Areas, _gamePlan.Connections, _aiColor);

        for (int j = 0; j < target.Count; ++j)
        {
          PrepareAttackFromInput(possibilities[i], input);
          PrepareAttackToInput(target[j], input);

          double[] result = _attackNetwork.Compute(input);
          while (result[0] > 0.5 && possibilities[i].SizeOfArmy > 1)
          {
            int prevAtt = possibilities[i].SizeOfArmy;
            int prevDeff = target[j].SizeOfArmy;

            AttackSize attackSize = GetAttackSize(result[1], prevAtt);

            MoveResult moveResult = _game.MakeMove(new Attack(_aiColor, possibilities[i].ID, target[j].ID, attackSize));

            input[0] -= (prevAtt - possibilities[i].SizeOfArmy);
            input[1] -= (prevDeff - target[j].SizeOfArmy);
            input[8] -= (prevDeff - target[j].SizeOfArmy);

            if (moveResult == MoveResult.AreaCaptured)
            {
              input[2]++;
              input[6]--;

              result = _attackNetwork.Compute(input);

              int armyToMove = (int)Math.Round((possibilities[i].SizeOfArmy - (int)attackSize - 1) * result[1]);
              armyToMove += (int)attackSize;

              MoveResult mr = _game.MakeMove(new Capture(_aiColor, armyToMove));

              if (armyToMove > 1)
              {
                possibilities.Add(target[j]);
              }

              bool bad = mr != MoveResult.OK;

              break;
            }
            else if (moveResult == MoveResult.Winner)
            {
              return;
            }
            else
            {
              bool bad = moveResult != MoveResult.OK;
            }

            result = _attackNetwork.Compute(input);
          }
        }
      }

      _sw.Stop();
      time[2] += _sw.Elapsed.TotalSeconds;
      count[2]++;
    }

    private AttackSize GetAttackSize(double result, int sizeOfArmy)
    {
      const double oneArmy = 0.33;
      const double twoArmies = 0.66;

      if (twoArmies <= result && 3 < sizeOfArmy)
      {
        return AttackSize.Three;
      }

      if (oneArmy <= result && result < twoArmies && 2 < sizeOfArmy)
      {
        return AttackSize.Two;
      }

      if (result < oneArmy)
      {
        return AttackSize.One;
      }

      return (AttackSize)(sizeOfArmy - 1);
    }

    private void PrepareAttackFromInput(Area area, double[] input)
    {
      const int levels = 2;

      input[0] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(area, levels, 2, input);
    }

    private void PrepareAttackToInput(Area area, double[] input)
    {
      const int levels = 2;

      input[1] = area.SizeOfArmy;

      int offset = 2 + levels * 4;

      input[offset] = _regionsInfo[area.RegionID].NumberOfAreas;
      offset++;

      Tuple<int, int> stateOfRegion = NeuroHelper.GetRegionState(_gamePlan.Areas, area.RegionID, _regionsInfo, _aiColor);

      input[offset] = stateOfRegion.Item1;
      offset++;
      input[offset] = stateOfRegion.Item2;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].BonusForRegion;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].NumberOfAreasForOneArmy;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].NumberOfDefendArmies;
      offset++;

      input[offset] = _regionsInfo[area.RegionID].DefendRate;
    }

    public void PlayFortify()
    {
      _sw.Restart();

      IList<Area> possibilities = Helper.WhoCanFortify(_gamePlan.Areas, _gamePlan.Connections, _aiColor);

      double[] input = new double[18];

      double max = 0;
      double army = 0;

      int fromID = 0;
      int toID = 0;

      for (int i = 0; i < possibilities.Count; ++i)
      {
        PrepareFortifyFromInput(possibilities[i], input);

        IList<Area> target = Helper.WhereCanFortify(possibilities[i], _gamePlan.Areas, _gamePlan.Connections, _aiColor);

        for (int j = 0; j < target.Count; ++j)
        {
          PrepareFortifyToInput(target[j], input);

          double[] result = _fortifyNetwork.Compute(input);

          if (max < result[0])
          {
            max = result[0];
            army = result[1];

            fromID = possibilities[i].ID;
            toID = target[j].ID;
          }
        }
      }

      if (max > 0.5)
      {
        int armyToMove = (int)Math.Round(army * (_gamePlan.Areas[fromID].SizeOfArmy - 1));

        if (armyToMove == 0)
        {
          armyToMove = 1;
        }

        MoveResult mr = _game.MakeMove(new Fortify(_aiColor, fromID, toID, armyToMove));

        bool bad = mr != MoveResult.OK;
      }

      _sw.Stop();
      time[3] += _sw.Elapsed.TotalSeconds;
      count[3]++;
    }

    private void PrepareFortifyFromInput(Area area, double[] input)
    {
      const int levels = 2;

      input[0] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(area, levels, 2, input);
    }

    private void PrepareFortifyToInput(Area area, double[] input)
    {
      const int levels = 2;

      input[1] = area.SizeOfArmy;

      AddSurroundingsInfoToInput(area, levels, 10, input);
    }

    private void AddSurroundingsInfoToInput(Area area, int levels, int offset, double[] input)
    {
      SurroundingsInformation info = NeuroHelper.GetSurroundingsInfo(area, _gamePlan.Areas, _gamePlan.Connections, levels, _aiColor);

      for (int i = 0; i < info.NumberOfFriends.Length; ++i)
      {
        input[offset + i] = info.NumberOfFriends[i];
      }
      offset += levels;

      for (int i = 0; i < info.FriendlyArmies.Length; ++i)
      {
        input[offset + i] = info.FriendlyArmies[i];
      }
      offset += levels;

      for (int i = 0; i < info.NumberOfEnemies.Length; ++i)
      {
        input[offset + i] = info.NumberOfEnemies[i];
      }
      offset += levels;

      for (int i = 0; i < info.EnemyArmies.Length; ++i)
      {
        input[offset + i] = info.EnemyArmies[i];
      }
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