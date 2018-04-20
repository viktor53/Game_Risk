using Risk.Model.GameCore;
using Risk.Model.GameCore.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Risk.Model.Enums;

namespace Risk.AI
{
  public class BattleOfAI
  {
    private bool _isClassic;

    private int _numberOfGames;

    private int _numberOfAreas;

    private Stopwatch _sw;

    private double _time = 0;

    public BattleOfAI(bool isClassic, int numberOfGames)
    {
      _isClassic = isClassic;
      _numberOfGames = numberOfGames;
      _numberOfAreas = 42;
      _sw = new Stopwatch();
    }

    public BattleOfAI(int numberOfAreas, int numberOfGames)
    {
      _isClassic = false;
      _numberOfGames = numberOfGames;
      _numberOfAreas = numberOfAreas;
      _sw = new Stopwatch();
    }

    public IDictionary<ArmyColor, int> PlaySimulationDiagnostic(IEnumerable<IAI> ais, TextWriter writer)
    {
      Dictionary<ArmyColor, int> wins = new Dictionary<ArmyColor, int>();

      foreach (var ai in ais)
      {
        wins.Add(ai.PlayerColor, 0);
      }

      _time = 0;

      for (int i = 0; i < _numberOfGames; ++i)
      {
        GameSimulation game;
        if (_isClassic)
        {
          game = new GameSimulation(_isClassic);
        }
        else
        {
          game = new GameSimulation(_numberOfAreas);
        }

        foreach (var ai in ais)
        {
          game.AddPlayer(ai);
        }

        _sw.Reset();
        _sw.Start();

        game.StartGame();

        _sw.Stop();
        var t = _sw.Elapsed;
        _time += t.TotalSeconds;

        foreach (var ai in ais)
        {
          if (ai.IsWinner)
          {
            wins[ai.PlayerColor]++;
          }
        }

        writer.WriteLine(i + " completed");
      }

      writer.WriteLine($"Time of all games: {_time}");
      writer.WriteLine($"Average time of game: {_time / _numberOfGames}");

      return wins;
    }

    public IDictionary<ArmyColor, double> PlaySimulation(IEnumerable<IAI> ais)
    {
      Dictionary<ArmyColor, double> wins = new Dictionary<ArmyColor, double>();

      foreach (var ai in ais)
      {
        wins.Add(ai.PlayerColor, 0);
      }

      for (int i = 0; i < _numberOfGames; ++i)
      {
        GameSimulation game;
        if (_isClassic)
        {
          game = new GameSimulation(_isClassic);
        }
        else
        {
          game = new GameSimulation(_numberOfAreas);
        }

        foreach (var ai in ais)
        {
          game.AddPlayer(ai);
        }

        game.StartGame();

        int count = 0;
        foreach (var ai in ais)
        {
          if (ai.IsWinner)
          {
            count++;
          }
        }

        foreach (var ai in ais)
        {
          if (ai.IsWinner)
          {
            wins[ai.PlayerColor] += 1 / (double)count;
          }
        }
      }

      return wins;
    }

    public IDictionary<ArmyColor, int> PlayRealGame(IEnumerable<IAI> ais)
    {
      Dictionary<ArmyColor, int> wins = new Dictionary<ArmyColor, int>();

      foreach (var ai in ais)
      {
        wins.Add(ai.PlayerColor, 0);
      }

      for (int i = 0; i < _numberOfGames; ++i)
      {
        Game game;
        if (_isClassic)
        {
          game = new Game(_isClassic, 3, null);
        }
        else
        {
          game = new Game(_numberOfAreas, 3, null);
        }

        foreach (var ai in ais)
        {
          game.AddPlayer(ai);
        }

        game.StartGame();

        foreach (var ai in ais)
        {
          if (ai.IsWinner)
          {
            wins[ai.PlayerColor]++;
          }
        }
      }

      return wins;
    }

    public IDictionary<ArmyColor, int> PlayRealGameDiagnostic(IEnumerable<IAI> ais, TextWriter writer)
    {
      Dictionary<ArmyColor, int> wins = new Dictionary<ArmyColor, int>();

      foreach (var ai in ais)
      {
        wins.Add(ai.PlayerColor, 0);
      }

      _time = 0;

      for (int i = 0; i < _numberOfGames; ++i)
      {
        Game game;
        if (_isClassic)
        {
          game = new Game(_isClassic, 3, null);
        }
        else
        {
          game = new Game(_numberOfAreas, 3, null);
        }

        foreach (var ai in ais)
        {
          game.AddPlayer(ai);
        }

        _sw.Reset();
        _sw.Start();

        game.StartGame();

        _sw.Stop();
        var t = _sw.Elapsed;
        _time += t.TotalSeconds;

        foreach (var ai in ais)
        {
          if (ai.IsWinner)
          {
            wins[ai.PlayerColor]++;
          }
        }

        writer.WriteLine(i + " completed");
      }

      writer.WriteLine($"Time of all games: {_time}");
      writer.WriteLine($"Average time of game: {_time / _numberOfGames}");

      return wins;
    }
  }
}