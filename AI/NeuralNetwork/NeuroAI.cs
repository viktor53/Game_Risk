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

    public NeuroAI(ArmyColor aiColor, bool isComplex, int generation)
    {
      _aiColor = aiColor;
      LoadNeuralNetwork(isComplex, generation);
      _cardsInHand = new List<RiskCard>();
    }

    private void LoadNeuralNetwork(bool isComplex, int generation)
    {
      if (isComplex)
      {
        if (generation == -1)
        {
          _setUpNetwork = NeuralNetworkFactory.CreateSetUpNetworkComplexTopology();
          _draftNetwork = NeuralNetworkFactory.CreateDraftNetworkComplexTopology();
          _exchangeCardNetwork = NeuralNetworkFactory.CreateExchangeNetworkComplexTopology();
          _attackNetwork = NeuralNetworkFactory.CreateAttackNetworkComplexTopology();
          _fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetworkComplexTopology();
        }
        else
        {
          _setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetworkComplex{generation}.ai");
          _draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetworkComplex{generation}.ai");
          _exchangeCardNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetworkComplex{generation}.ai");
          _attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetworkComplex{generation}.ai");
          _fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetworkComplex{generation}.ai");
        }
      }
      else
      {
        if (generation == -1)
        {
          _setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
          _draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
          _exchangeCardNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
          _attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
          _fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();
        }
        else
        {
          _setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetwork{generation}.ai");
          _draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetwork{generation}.ai");
          _exchangeCardNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetwork{generation}.ai");
          _attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetwork{generation}.ai");
          _fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetwork{generation}.ai");
        }
      }
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
      //_sw.Restart();

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
        InputBuilder.PrepareSetUpInput(_aiColor, area, _gamePlan.Areas, _gamePlan.Connections, _regionsInfo, input);

        double result = _setUpNetwork.Compute(input)[0];

        if (result > max)
        {
          max = result;
          best = area.ID;
        }
      }

      MoveResult mr = _game.MakeMove(new SetUp(_aiColor, best));

      bool bad = mr != MoveResult.OK;

      //_sw.Stop();
      //time[0] += _sw.Elapsed.TotalSeconds;
      //count[0]++;
    }

    public void PlayDraft()
    {
      //_sw.Restart();

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

          InputBuilder.PrepareExchangeCardInput(_aiColor, FreeUnit, _game.GetNumberOfCombination(), _gamePlan.Areas, _gamePlan.Connections, inputEx);

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
          InputBuilder.PrepareDraftInput(_aiColor, possibilities[i], _gamePlan.Areas, _gamePlan.Connections, input);

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

      //_sw.Stop();
      //time[1] += _sw.Elapsed.TotalSeconds;
      //count[1]++;
    }

    public void PlayAttack()
    {
      //_sw.Restart();

      IList<Area> possibilities = Helper.WhoCanAttack(_gamePlan.Areas, _gamePlan.Connections, _aiColor);

      double[] input = new double[17];

      for (int i = 0; i < possibilities.Count; ++i)
      {
        IList<Area> target = Helper.WhoCanBeAttacked(possibilities[i], _gamePlan.Areas, _gamePlan.Connections, _aiColor);

        for (int j = 0; j < target.Count; ++j)
        {
          InputBuilder.PrepareAttackFromInput(_aiColor, possibilities[i], _gamePlan.Areas, _gamePlan.Connections, input);
          InputBuilder.PrepareAttackToInput(_aiColor, target[j], _gamePlan.Areas, _regionsInfo, input);

          double[] result = _attackNetwork.Compute(input);
          while (result[0] > 0.5 && possibilities[i].SizeOfArmy > 1)
          {
            int prevAtt = possibilities[i].SizeOfArmy;
            int prevDeff = target[j].SizeOfArmy;

            AttackSize attackSize = (AttackSize)InputBuilder.GetAttackSize(result[1], prevAtt);

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

      //_sw.Stop();
      //time[2] += _sw.Elapsed.TotalSeconds;
      //count[2]++;
    }

    public void PlayFortify()
    {
      //_sw.Restart();

      IList<Area> possibilities = Helper.WhoCanFortify(_gamePlan.Areas, _gamePlan.Connections, _aiColor);

      double[] input = new double[18];

      double max = 0;
      double army = 0;

      int fromID = 0;
      int toID = 0;

      for (int i = 0; i < possibilities.Count; ++i)
      {
        InputBuilder.PrepareFortifyFromInput(_aiColor, possibilities[i], _gamePlan.Areas, _gamePlan.Connections, input);

        IList<Area> target = Helper.WhereCanFortify(possibilities[i], _gamePlan.Areas, _gamePlan.Connections, _aiColor);

        for (int j = 0; j < target.Count; ++j)
        {
          InputBuilder.PrepareFortifyToInput(_aiColor, target[j], _gamePlan.Areas, _gamePlan.Connections, input);

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

      //_sw.Stop();
      //time[3] += _sw.Elapsed.TotalSeconds;
      //count[3]++;
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