using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Networking.Server;
using Risk.Model.GamePlan;
using Risk.Model.GameCore.Loggers;
using log4net.Config;
using System.IO;
using Risk.Networking.Messages.Data;
using System.Threading;
using Risk.Model.GameCore.Moves;

namespace Risk.Networking.Client
{
  /// <summary>
  /// Represents offline client.
  /// </summary>
  public class RiskOfflineClient : IPlayer
  {
    private OfflinePlayer _player;

    private event EventHandler _onUpdate;

    private event EventHandler _onYourTurn;

    private event EventHandler _onMoveresult;

    private event EventHandler _onFreeUnit;

    private event EventHandler _onArmyColor;

    private event EventHandler _onUpdateCard;

    private event EventHandler _onEndGame;

    /// <summary>
    /// OnArmyColor is raised whenever player of color is changed.
    /// </summary>
    public event EventHandler OnArmyColor
    {
      add
      {
        _onArmyColor = null;
        _onArmyColor += value;
      }
      remove
      {
        _onArmyColor -= value;
      }
    }

    /// <summary>
    /// OnEndGame is raised whenever game is ended.
    /// </summary>
    public event EventHandler OnEndGame
    {
      add
      {
        _onEndGame = null;
        _onEndGame += value;
      }
      remove
      {
        _onEndGame -= value;
      }
    }

    /// <summary>
    /// OnFreeUnit is raised whenever number of free unit is changed.
    /// </summary>
    public event EventHandler OnFreeUnit
    {
      add
      {
        _onFreeUnit = null;
        _onFreeUnit += value;
      }
      remove
      {
        _onFreeUnit -= value;
      }
    }

    /// <summary>
    /// OnMoveResult is raised whenever move result is reveived from game.
    /// </summary>
    public event EventHandler OnMoveResult
    {
      add
      {
        _onMoveresult = null;
        _onMoveresult += value;
      }
      remove
      {
        _onMoveresult = null;
      }
    }

    /// <summary>
    /// OnUpdate is raised whenever update is received.
    /// (update game board)
    /// </summary>
    public event EventHandler OnUpdate
    {
      add
      {
        _onUpdate = null;
        _onUpdate += value;
      }
      remove
      {
        _onUpdate -= value;
      }
    }

    /// <summary>
    /// OnUpdateCard is raised whenever number of cards is changed.
    /// </summary>
    public event EventHandler OnUpdateCard
    {
      add
      {
        _onUpdateCard = null;
        _onUpdateCard += value;
      }
      remove
      {
        _onUpdateCard -= value;
      }
    }

    /// <summary>
    /// OnYourTurn is raised whenever player is playing now.
    /// </summary>
    public event EventHandler OnYourTurn
    {
      add
      {
        _onYourTurn = null;
        _onYourTurn += value;
      }
      remove
      {
        _onYourTurn -= value;
      }
    }

    private class OfflinePlayer : Model.GameCore.IPlayer
    {
      private RiskOfflineClient _client;

      private IGame _game;

      private ArmyColor _playerColor;

      private int _freeUnit;

      private IList<RiskCard> _cards;

      private bool _nextPhase;

      public IGame Game => _game;

      public IList<RiskCard> Cards => _cards;

      public int FreeUnit
      {
        get
        {
          return _freeUnit;
        }

        set
        {
          _freeUnit = value;
          _client._onFreeUnit?.Invoke(this, new FreeUnitEventArgs(_freeUnit));
        }
      }

      public ArmyColor PlayerColor
      {
        get
        {
          return _playerColor;
        }

        set
        {
          _playerColor = value;
        }
      }

      public bool NexthPhase
      {
        get
        {
          return _nextPhase;
        }
        set
        {
          _nextPhase = value;
        }
      }

      public OfflinePlayer(IGame game, RiskOfflineClient client, ArmyColor playerColor)
      {
        _client = client;
        _game = game;
        _game.AddPlayer(this);
        _playerColor = playerColor;
        _cards = new List<RiskCard>();
      }

      public void AddCard(RiskCard card)
      {
        _cards.Add(card);
        _client._onUpdateCard?.Invoke(this, new UpdateCardEventArgs(true));
      }

      public void RemoveCard(RiskCard card)
      {
        _cards.Remove(card);
        _client._onUpdateCard?.Invoke(this, new UpdateCardEventArgs(false));
      }

      public async Task EndPlayer(bool isWinner)
      {
        await Task.Run(() =>
        {
          _client._onEndGame?.Invoke(this, new EndGameEventArgs(isWinner));
        });
      }

      public void PlayAttack()
      {
        while (!_nextPhase)
        {
          Thread.Sleep(1000);
        }
        _nextPhase = false;
      }

      public void PlayDraft()
      {
        _client._onYourTurn?.Invoke(this, new ConfirmationEventArgs(false));
        while (!_nextPhase)
        {
          Thread.Sleep(1000);
        }
        _nextPhase = false;
      }

      public void PlayFortify()
      {
        while (!_nextPhase)
        {
          Thread.Sleep(1000);
        }
        _nextPhase = false;
      }

      public void PlaySetUp()
      {
        _client._onYourTurn?.Invoke(this, new ConfirmationEventArgs(true));
        while (!_nextPhase)
        {
          Thread.Sleep(1000);
        }
        _nextPhase = false;
      }

      public async Task StartPlayer(IGame game)
      {
        await Task.Run(() =>
        {
          _game = game;
          _client._onArmyColor?.Invoke(this, new ArmyColorEventArgs((long)_playerColor));
        });
      }

      public async Task UpdateGame(byte areaID, ArmyColor armyColor, int sizeOfArmy)
      {
        await Task.Run(() =>
        {
          var area = new Area(areaID, 0);
          area.ArmyColor = armyColor;
          area.SizeOfArmy = sizeOfArmy;
          _client._onUpdate?.Invoke(this, new UpdateGameEventArgs(area));
        });
      }
    }

    public RiskOfflineClient(IGame game, ArmyColor playerColor)
    {
      _player = new OfflinePlayer(game, this, playerColor);
    }

    public Task<bool> ListenToGameCommandsAsync()
    {
      return Task.Run(() =>
      {
        _player.Game.StartGame();

        return true;
      });
    }

    public async Task<bool> SendAttackMoveAsync(ArmyColor playerColor, int attackerAreaID, int defenderAreaID, AttackSize attackSize)
    {
      return await Task.Run(() =>
      {
        var result = _player.Game.MakeMove(new Attack(playerColor, attackerAreaID, defenderAreaID, attackSize));
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)result));
        return true;
      });
    }

    public async Task<bool> SendCaptureMoveAsync(ArmyColor playerColor, int armyToMove)
    {
      return await Task.Run(() =>
      {
        var result = _player.Game.MakeMove(new Capture(playerColor, armyToMove));
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)result));
        return true;
      });
    }

    public async Task<bool> SendDraftMoveAsync(ArmyColor playerColor, int areaID, int numberOfUnit)
    {
      return await Task.Run(() =>
      {
        var result = _player.Game.MakeMove(new Draft(playerColor, areaID, numberOfUnit));
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)result));
        return true;
      });
    }

    public async Task<bool> SendExchangeCardAsync(ArmyColor playerColor)
    {
      return await Task.Run(() =>
      {
        var result = _player.Game.MakeMove(new ExchangeCard(playerColor, CombinationPicker.GetCombination(_player.Cards)));
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)result));
        return true;
      });
    }

    public async Task<bool> SendFortifyMoveAsync(ArmyColor playerColor, int fromAreaID, int toAreaID, int sizeOfArmy)
    {
      return await Task.Run(() =>
      {
        var result = _player.Game.MakeMove(new Fortify(playerColor, fromAreaID, toAreaID, sizeOfArmy));
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)result));
        return true;
      });
    }

    public async Task<bool> SendNextPhaseAsync()
    {
      return await Task.Run(() =>
      {
        _player.NexthPhase = true;
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)MoveResult.OK));
        return true;
      });
    }

    public async Task<bool> SendSetUpMoveAsync(ArmyColor playerColor, int idArea)
    {
      return await Task.Run(() =>
      {
        var result = _player.Game.MakeMove(new SetUp(playerColor, idArea));
        _player.NexthPhase = result == MoveResult.OK;
        _onMoveresult?.Invoke(this, new MoveResultEventArgs((long)result));
        return true;
      });
    }

    public GameBoardInfo GetGameBoardInfo()
    {
      return GameRoom.CreateBoardInfo(_player.Game.GetGamePlan());
    }

    public static IGame GetGame(bool isClassic, int numberOfPlayers, FileInfo pathToConfig)
    {
      XmlConfigurator.Configure(pathToConfig);

      if (isClassic)
      {
        return new Game(isClassic, numberOfPlayers, Loggers.GetDateFileLogger(AppDomain.CurrentDomain.BaseDirectory + "Logs\\Games"));
      }
      else
      {
        return new Game(15, numberOfPlayers, Loggers.GetDateFileLogger(AppDomain.CurrentDomain.BaseDirectory + "Logs\\Games"));
      }
    }
  }
}