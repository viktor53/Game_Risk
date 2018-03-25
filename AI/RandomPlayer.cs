using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GamePlan;
using Risk.Model.GameCore.Moves;

namespace Risk.AI
{
  internal class RandomPlayer : IPlayer
  {
    private int _freeUnit;

    public ArmyColor _aiColor;

    private List<RiskCard> _cardsInHand;

    private IGame _game;

    private GamePlanInfo _gamePlan;

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
      _ran = new Random();
    }

    public RandomPlayer(ArmyColor playerColor)
    {
      _aiColor = playerColor;
    }

    public async Task StartPlayer(IGame game)
    {
      await Task.Run(() =>
      {
        _game = game;
        _gamePlan = game.GetGamePlan();
        _cardsInHand = new List<RiskCard>();
      });
    }

    public void PlayAttack()
    {
      IList<Area> canAttack = Helper.WhoCanAttack(_gamePlan, _aiColor);

      int probibility = _ran.Next(100);
      while (probibility >= 50 && canAttack.Count > 0)
      {
        int attacker = _ran.Next(canAttack.Count);
        IList<Area> canBeAttacked = Helper.WhoCanBeAttacked(canAttack[attacker], _gamePlan, _aiColor);

        int defender = _ran.Next(canBeAttacked.Count);
        int attackSize = _ran.Next(1, Helper.GetMaxSizeOfAttack(canAttack[attacker]) + 1);
        MoveResult result = _game.MakeMove(new Attack(_aiColor, canAttack[attacker].ID, canBeAttacked[defender].ID, (AttackSize)attackSize));

        if (result == MoveResult.AreaCaptured)
        {
          _game.MakeMove(new Capture(_aiColor, _ran.Next(attackSize, canAttack[attacker].SizeOfArmy)));
          canBeAttacked.Remove(canBeAttacked[defender]);
        }

        if (result == MoveResult.Winner)
        {
          return;
        }

        if (canAttack[attacker].SizeOfArmy == 1 || canBeAttacked.Count == 0)
        {
          canAttack.Remove(canBeAttacked[attacker]);
        }

        probibility = _ran.Next(100);
      }
    }

    public void PlayDraft()
    {
      while (_cardsInHand.Count >= 5)
      {
        IList<RiskCard> combination = Helper.GetCombination(_cardsInHand);
        _game.MakeMove(new ExchangeCard(_aiColor, combination));
      }

      if (_cardsInHand.Count >= 3)
      {
        int probibility = _ran.Next(100);
        if (probibility >= 50)
        {
          IList<RiskCard> combination = Helper.GetCombination(_cardsInHand);
          _game.MakeMove(new ExchangeCard(_aiColor, combination));
        }
      }

      IList<Area> posibilities = Helper.GetMyAreas(_gamePlan.Areas, _aiColor);

      while (FreeUnit > 0)
      {
        int result = _ran.Next(posibilities.Count);
        int numberOfarmies = _ran.Next(FreeUnit + 1);
        _game.MakeMove(new Draft(_aiColor, posibilities[result].ID, numberOfarmies));
      }
    }

    public void PlayFortify()
    {
      int probibility = _ran.Next(100);
      if (probibility >= 50)
      {
        IList<Area> canFortify = Helper.WhoCanFortify(_gamePlan, _aiColor);
        if (canFortify.Count > 0)
        {
          int from = _ran.Next(canFortify.Count);
          IList<Area> where = Helper.WhereCanFortify(canFortify[from], _gamePlan, _aiColor);

          int to = _ran.Next(where.Count);
          int sizeOfArmy = _ran.Next(1, canFortify[from].SizeOfArmy);
          _game.MakeMove(new Fortify(_aiColor, canFortify[from].ID, where[to].ID, sizeOfArmy));
        }
      }
    }

    public void PlaySetUp()
    {
      IList<Area> possibilities = Helper.GetUnoccupiedAreas(_gamePlan.Areas);
      if (possibilities.Count == 0)
      {
        possibilities = Helper.GetMyAreas(_gamePlan.Areas, PlayerColor);
      }

      int result = _ran.Next(possibilities.Count);
      _game.MakeMove(new SetUp(_aiColor, possibilities[result].ID));
    }

    public async Task EndPlayer(bool isWinner)
    {
      await Task.Run(() =>
      {
        _isWinner = isWinner;
      });
    }

    public async Task UpdateGame(Area area)
    {
      await Task.Run(() =>
      {
        _gamePlan.Areas[area.ID] = area;
      });
    }
  }
}