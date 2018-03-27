using Risk.Model.GameCore;
using Risk.Model.GameCore.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Risk.AI
{
  public class BattleOfAI
  {
    private IEnumerable<IAI> _ais;

    private int _count;

    private Dictionary<IPlayer, int> _wins;

    private bool _isClassic;

    private int _numberOfGames;

    private Stopwatch _sw;

    private double _time = 0;

    public BattleOfAI(IEnumerable<IAI> ais, bool isClassic, int numberOfGames)
    {
      _ais = ais;
      _wins = new Dictionary<IPlayer, int>();
      _count = 0;
      foreach (var ai in ais)
      {
        _wins.Add(ai, 0);
        _count++;
      }
      _isClassic = isClassic;
      _numberOfGames = numberOfGames;
      _sw = new Stopwatch();
    }

    public IDictionary<IPlayer, int> Play(TextWriter writer)
    {
      GC.Collect(3, GCCollectionMode.Forced, true);

      _time = 0;
      for (int i = 0; i < _numberOfGames; ++i)
      {
        GameSimulation game = new GameSimulation(_isClassic);
        foreach (var ai in _ais)
        {
          game.AddPlayer(ai);
        }

        _sw.Reset();
        _sw.Start();

        game.StartGame();

        _sw.Stop();
        var t = _sw.Elapsed;
        _time += t.TotalSeconds;

        foreach (var ai in _ais)
        {
          if (ai.IsWinner)
          {
            _wins[ai]++;
          }
        }
      }

      writer.WriteLine($"Time of all games: {_time}");
      writer.WriteLine($"Average time of game: {_time / _numberOfGames}");

      return _wins;
    }
  }
}