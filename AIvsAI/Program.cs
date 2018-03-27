using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.AI;
using Risk.Model.Enums;

namespace AIvsAI
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      List<RandomPlayer> rps = new List<RandomPlayer>();
      rps.Add(new RandomPlayer(ArmyColor.Green));
      rps.Add(new RandomPlayer(ArmyColor.Red));
      rps.Add(new RandomPlayer(ArmyColor.Blue));

      List<IAI> ais = new List<IAI>();
      foreach (var player in rps)
      {
        ais.Add(player);
      }

      BattleOfAI battle = new BattleOfAI(ais, true, 1000);

      Console.WriteLine($"Probibility of Attack: 85");

      var result = battle.Play(Console.Out);

      //Console.WriteLine($"Green AI: {result[ais[0]]}");
      //Console.WriteLine($"**** Avarage SetUp time: {ais[0].GetAvgTimeSetUp}");
      //Console.WriteLine($"**** Avarage Draft time: {ais[0].GetAvgTimeDraft}");
      //Console.WriteLine($"**** Avarage Attack time: {ais[0].GetAvgTimeAttack}");
      //Console.WriteLine($"**** Avarage Fortify time: {ais[0].GetAvgTimeFortify}");

      //Console.WriteLine($"Red AI: {result[ais[1]]}");
      //Console.WriteLine($"**** Avarage SetUp time: {ais[1].GetAvgTimeSetUp}");
      //Console.WriteLine($"**** Avarage Draft time: {ais[1].GetAvgTimeDraft}");
      //Console.WriteLine($"**** Avarage Attack time: {ais[1].GetAvgTimeAttack}");
      //Console.WriteLine($"**** Avarage Fortify time: {ais[1].GetAvgTimeFortify}");

      //Console.WriteLine($"Blue AI: {result[ais[2]]}");
      //Console.WriteLine($"**** Avarage SetUp time: {ais[2].GetAvgTimeSetUp}");
      //Console.WriteLine($"**** Avarage Draft time: {ais[2].GetAvgTimeDraft}");
      //Console.WriteLine($"**** Avarage Attack time: {ais[2].GetAvgTimeAttack}");
      //Console.WriteLine($"**** Avarage Fortify time: {ais[2].GetAvgTimeFortify}");
    }
  }
}