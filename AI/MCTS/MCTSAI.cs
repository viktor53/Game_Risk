using Risk.AI.NeuralNetwork;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  /// <summary>
  /// Player who uses monte carlo tree search to making move.
  /// </summary>
  public class MCTSAI : IAI
  {
    private int _freeUnit;

    public ArmyColor _aiColor;

    private List<RiskCard> _cardsInHand;

    private IGame _game;

    private GamePlanInfo _gamePlan;

    private bool _isWinner;

    private Random _ran;

    private bool _isWithNN;

    private MonteCarloTreeSearch _mcts;

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

    public double GetAvgTimeSetUp => time[0] / count[0];

    public double GetAvgTimeDraft => time[1] / count[1];

    public double GetAvgTimeAttack => time[2] / count[2];

    public double GetAvgTimeFortify => time[3] / count[3];

    private Stopwatch _sw = new Stopwatch();

    private double[] time = new double[] { 0, 0, 0, 0 };

    private int[] count = new int[] { 0, 0, 0, 0 };

    private Moves moves;

    public void AddCard(RiskCard card)
    {
      _cardsInHand.Add(card);
    }

    public void RemoveCard(RiskCard card)
    {
      _cardsInHand.Remove(card);
    }

    public MCTSAI(ArmyColor playerColor, bool isWithNN)
    {
      PlayerColor = playerColor;
      _ran = new Random();
      _cardsInHand = new List<RiskCard>();
      _isWithNN = isWithNN;
    }

    public async Task StartPlayer(IGame game)
    {
      await Task.Run(() =>
      {
        _game = game;
        _gamePlan = game.GetGamePlan();
        var orderedPlayers = _game.GetOrderOfPlayers();
        if (_isWithNN)
        {
          var regionsInfo = NeuroHelper.GetRegionsInformation(_gamePlan.Areas, _gamePlan.Connections, _game.GetBonusForRegions());
          _mcts = new MonteCarloTreeSearch(orderedPlayers.Count, orderedPlayers, new NeuroHeuristicHelper(true, 3, regionsInfo));
        }
        else
        {
          _mcts = new MonteCarloTreeSearch(orderedPlayers.Count, orderedPlayers);
        }
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

    public async Task UpdateGame(byte areaID, ArmyColor armyColor, int sizeOfArmy)
    {
      await Task.Run(() =>
      {
        _gamePlan.Areas[areaID].ArmyColor = armyColor;
        _gamePlan.Areas[areaID].SizeOfArmy = sizeOfArmy;
      });
    }

    public void PlaySetUp()
    {
      _sw.Restart();

      while (_gamePlan == null)
      {
        Thread.Sleep(5);
      }

      Moves move = _mcts.GetNextMove(_game.GetCurrentStateOfGameBoard(), _game.GetPlayersInfo(), _game.GetCurrentPlayer(), Phase.SETUP);

      _game.MakeMove(move.SetUpMove);

      _sw.Stop();
      time[0] += _sw.Elapsed.TotalSeconds;
      count[0]++;
    }

    public void PlayDraft()
    {
      _sw.Restart();

      moves = _mcts.GetNextMove(_game.GetCurrentStateOfGameBoard(), _game.GetPlayersInfo(), _game.GetCurrentPlayer(), Phase.DRAFT);

      for (int i = 0; i < moves.CardMoves.Count; ++i)
      {
        _game.MakeMove(moves.CardMoves[i]);
      }

      for (int i = 0; i < moves.DraftMoves.Count; ++i)
      {
        _game.MakeMove(moves.DraftMoves[i]);
      }

      _sw.Stop();
      time[1] += _sw.Elapsed.TotalSeconds;
      count[1]++;
    }

    public void PlayAttack()
    {
      _sw.Restart();

      int capture = 0;
      for (int i = 0; i < moves.AttackMoves.Count; ++i)
      {
        if (IsCorrectAttack(moves.AttackMoves[i]))
        {
          MoveResult result = _game.MakeMove(moves.AttackMoves[i]);

          if (result == MoveResult.AreaCaptured)
          {
            if (capture < moves.CaptureMoves.Count)
            {
              _game.MakeMove(CheckCaptureMove(moves.CaptureMoves[capture], moves.AttackMoves[i]));
              capture++;
            }
            else
            {
              _game.MakeMove(new Capture(PlayerColor, (int)moves.AttackMoves[i].AttackSize));
            }
          }
          else if (result == MoveResult.Winner)
          {
            return;
          }
        }
      }

      _sw.Stop();
      time[2] += _sw.Elapsed.TotalSeconds;
      count[2]++;
    }

    private bool IsCorrectAttack(Attack attack)
    {
      Area attacker = _gamePlan.Areas[attack.AttackerAreaID];
      if (attacker.SizeOfArmy > 1 && (int)attack.AttackSize < attacker.SizeOfArmy)
      {
        if (_gamePlan.Areas[attack.DefenderAreaID].ArmyColor != PlayerColor)
        {
          return true;
        }
      }
      return false;
    }

    private Capture CheckCaptureMove(Capture move, Attack previousAttack)
    {
      int armyToMove = move.ArmyToMove;
      if (move.ArmyToMove < (int)previousAttack.AttackSize)
      {
        armyToMove = (int)previousAttack.AttackSize;
      }
      else if (move.ArmyToMove >= _gamePlan.Areas[previousAttack.AttackerAreaID].SizeOfArmy)
      {
        armyToMove = _gamePlan.Areas[previousAttack.AttackerAreaID].SizeOfArmy - 1;
      }

      return new Capture(move.PlayerColor, armyToMove);
    }

    public void PlayFortify()
    {
      _sw.Restart();

      Fortify move = CheckFortify(moves.FortifyMove);
      if (move != null)
      {
        _game.MakeMove(move);
      }

      _sw.Stop();
      time[3] += _sw.Elapsed.TotalSeconds;
      count[3]++;
    }

    private Fortify CheckFortify(Fortify move)
    {
      if (move == null)
      {
        return null;
      }

      if (_gamePlan.Areas[move.ToAreaID].ArmyColor != PlayerColor)
      {
        return null;
      }

      int sizeOfArmy = _gamePlan.Areas[move.FromAreaID].SizeOfArmy;

      if (sizeOfArmy <= 1)
      {
        return null;
      }

      if (sizeOfArmy <= move.SizeOfArmy)
      {
        return new Fortify(PlayerColor, move.FromAreaID, move.ToAreaID, sizeOfArmy - 1);
      }

      return move;
    }
  }
}