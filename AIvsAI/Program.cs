using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.AI;
using Risk.Model.Enums;
using Risk.AI.MCTS;
using Risk.AI.NeuralNetwork;
using Risk.AI.NeuralNetwork.Evolution;
using Accord.Neuro;

namespace AIvsAI
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      MainMenu();
    }

    private static void MainMenu()
    {
      Console.Clear();
      Console.WriteLine("**** Menu ****");
      Console.WriteLine("** 1 - Learn");
      Console.WriteLine("** 2 - Battle");
      Console.WriteLine();
      Console.Write("Choice: ");

      int choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        switch (choice)
        {
          case 1:
            LearnMenu();
            break;

          case 2:
            BattleMenu();
            break;

          default:
            Console.WriteLine("Invalid choice!");
            break;
        }
      }
      else
      {
        Console.WriteLine("Not a number!");
      }
    }

    private static void LearnMenu()
    {
      Console.Clear();
      Console.WriteLine("**** Learn Menu ****");
      Console.WriteLine("** 1 - Learn from start");
      Console.WriteLine("** 2 - Learn from already learnt NN");
      Console.WriteLine();
      Console.Write("Choice: ");

      int choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        switch (choice)
        {
          case 1:
            LearnFromStart();
            break;

          case 2:
            LearnSettings();
            break;

          default:
            Console.WriteLine("Invalid choice!");
            break;
        }
      }
      else
      {
        Console.WriteLine("Not a number!");
      }
    }

    private static void LearnSettings()
    {
      Console.Clear();
      Console.WriteLine("**** Learn Settings ****");
      Console.Write("Number of NN generation to load: ");

      int gen = 0;
      int nextGen = 0;

      int choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        gen = choice;
      }
      else
      {
        Console.WriteLine("Not a number!");
        return;
      }

      Console.Write("Number of NN next generation to save: ");

      if (int.TryParse(Console.ReadLine(), out choice))
      {
        nextGen = choice;
      }
      else
      {
        Console.WriteLine("Not a number!");
        return;
      }

      Learn(gen, nextGen);
    }

    private static void BattleMenu()
    {
      Console.Clear();
      Console.WriteLine("**** Battle Menu ****");
      Console.WriteLine("** 1 - Random vs Random vs Random");
      Console.WriteLine("** 2 - MCTS vs Random vs Random");
      Console.WriteLine("** 3 - NN vs Random vs Random");
      Console.WriteLine("** 4 - NN vs NN vs NN");
      Console.WriteLine("** 5 - MCTS vs NN vs Random");
      Console.WriteLine();
      Console.Write("Choice: ");

      int choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        switch (choice)
        {
          case 1:
            Battle(GetRandomOnly(), "Random agent", "Random agent", "Random agent");
            break;

          case 2:
            Battle(GetMCTSAndRandom(), "MCTS agent", "Random agent", "Random agent");
            break;

          case 3:
            Battle(GetNNAndRandom(), "NN agent", "Random agent", "Random agent");
            break;

          case 4:
            Battle(GetNNOnly(), "NN agent 1", "NN agent 2", "NN agent 3");
            break;

          case 5:
            Battle(GetMCTSAndNNAndRandom(), "MCTS agent", "NN agent", "Random agent");
            break;

          default:
            Console.WriteLine("Invalid choice!");
            break;
        }
      }
      else
      {
        Console.WriteLine("Not a number!");
      }
    }

    private static void Battle(IList<IAI> agents, string firstName, string secondName, string thirdName)
    {
      Console.Clear();

      BattleOfAI battle = new BattleOfAI(false, 200);

      var result = battle.PlaySimulationDiagnostic(agents, Console.Out);

      Console.WriteLine($"{firstName} agent: {result[agents[0].PlayerColor]}");
      Console.WriteLine($"**** Average SetUp time: {agents[0].GetAvgTimeSetUp}");
      Console.WriteLine($"**** Average Draft time: {agents[0].GetAvgTimeDraft}");
      Console.WriteLine($"**** Average Attack time: {agents[0].GetAvgTimeAttack}");
      Console.WriteLine($"**** Average Fortify time: {agents[0].GetAvgTimeFortify}");

      Console.WriteLine($"{secondName} agent: {result[agents[1].PlayerColor]}");
      Console.WriteLine($"**** Average SetUp time: {agents[1].GetAvgTimeSetUp}");
      Console.WriteLine($"**** Average Draft time: {agents[1].GetAvgTimeDraft}");
      Console.WriteLine($"**** Average Attack time: {agents[1].GetAvgTimeAttack}");
      Console.WriteLine($"**** Average Fortify time: {agents[1].GetAvgTimeFortify}");

      Console.WriteLine($"{thirdName} agent: {result[agents[2].PlayerColor]}");
      Console.WriteLine($"**** Average SetUp time: {agents[2].GetAvgTimeSetUp}");
      Console.WriteLine($"**** Average Draft time: {agents[2].GetAvgTimeDraft}");
      Console.WriteLine($"**** Average Attack time: {agents[2].GetAvgTimeAttack}");
      Console.WriteLine($"**** Average Fortify time: {agents[2].GetAvgTimeFortify}");
    }

    private static IList<IAI> GetRandomOnly()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(new RandomPlayer(ArmyColor.Green));
      agents.Add(new RandomPlayer(ArmyColor.Red));
      agents.Add(new RandomPlayer(ArmyColor.Blue));

      return agents;
    }

    private static IList<IAI> GetMCTSAndRandom()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(new MCTSAI(ArmyColor.Green));
      agents.Add(new RandomPlayer(ArmyColor.Red));
      agents.Add(new RandomPlayer(ArmyColor.Blue));

      return agents;
    }

    private static IList<IAI> GetNNAndRandom()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(LoadNN(ArmyColor.Green));

      agents.Add(new RandomPlayer(ArmyColor.Red));
      agents.Add(new RandomPlayer(ArmyColor.Blue));

      return agents;
    }

    private static IList<IAI> GetNNOnly()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(LoadNN(ArmyColor.Green));
      agents.Add(LoadNN(ArmyColor.Red));
      agents.Add(LoadNN(ArmyColor.Blue));

      return agents;
    }

    private static IList<IAI> GetMCTSAndNNAndRandom()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(new MCTSAI(ArmyColor.Green));
      agents.Add(LoadNN(ArmyColor.Red));
      agents.Add(new RandomPlayer(ArmyColor.Blue));

      return agents;
    }

    private static IAI LoadNN(ArmyColor agentColor)
    {
      Console.Write("Number of NN generation to load: ");

      int generation = 0;

      int choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        generation = choice;

        if (generation == -1)
        {
          return new NeuroAI(agentColor, NeuralNetworkFactory.CreateSetUpNetwork(), NeuralNetworkFactory.CreateDraftNetwork(),
        NeuralNetworkFactory.CreateExchangeNetwork(), NeuralNetworkFactory.CreateAttackNetwork(), NeuralNetworkFactory.CreateFortifyNetwork());
        }
        else
        {
          ActivationNetwork setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetwork{generation}.ai");
          ActivationNetwork draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetwork{generation}.ai");
          ActivationNetwork exchangeNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetwork{generation}.ai");
          ActivationNetwork attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetwork{generation}.ai");
          ActivationNetwork fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetwork{generation}.ai");

          return new NeuroAI(ArmyColor.Green, setUpNetwork, draftNetwork, exchangeNetwork, attackNetwork, fortifyNetwork);
        }
      }
      else
      {
        throw new ArgumentException("Not a number!");
      }
    }

    private static void LearnFromStart()
    {
      IAI enemy1 = new NeuroAI(ArmyColor.Red, NeuralNetworkFactory.CreateSetUpNetwork(), NeuralNetworkFactory.CreateDraftNetwork(),
        NeuralNetworkFactory.CreateExchangeNetwork(), NeuralNetworkFactory.CreateAttackNetwork(), NeuralNetworkFactory.CreateFortifyNetwork());
      IAI enemy2 = new NeuroAI(ArmyColor.Blue, NeuralNetworkFactory.CreateSetUpNetwork(), NeuralNetworkFactory.CreateDraftNetwork(),
        NeuralNetworkFactory.CreateExchangeNetwork(), NeuralNetworkFactory.CreateAttackNetwork(), NeuralNetworkFactory.CreateFortifyNetwork());

      NNFitnessFuction fitnessFunc = new NNFitnessFuction(enemy1, enemy2, false, 200);

      Console.Clear();
      Console.WriteLine("**** Learn starts ****");

      double[] best = Learning.Learn(100, 60, fitnessFunc, 0.5, 0.01, Console.Out);

      Console.WriteLine("**** Learn ends ****");

      ActivationNetwork setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
      ActivationNetwork draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
      ActivationNetwork exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
      ActivationNetwork attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
      ActivationNetwork fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();

      int index = 0;
      index = Learning.SetWeights(setUpNetwork, index, best);
      index = Learning.SetWeights(draftNetwork, index, best);
      index = Learning.SetWeights(exchangeNetwork, index, best);
      index = Learning.SetWeights(attackNetwork, index, best);
      Learning.SetWeights(fortifyNetwork, index, best);

      string baseDir = AppDomain.CurrentDomain.BaseDirectory;
      setUpNetwork.Save(baseDir + "\\AI\\setUpNetwork0.ai");
      draftNetwork.Save(baseDir + "\\AI\\draftnetwork0.ai");
      exchangeNetwork.Save(baseDir + "\\AI\\exchangeCardNetwork0.ai");
      attackNetwork.Save(baseDir + "\\AI\\attackNetwork0.ai");
      fortifyNetwork.Save(baseDir + "\\AI\\fortifyNetwork0.ai");
    }

    private static void Learn(int generation, int nextGeneration)
    {
      ActivationNetwork setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetwork{generation}.ai");
      ActivationNetwork draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetwork{generation}.ai");
      ActivationNetwork exchangeNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetwork{generation}.ai");
      ActivationNetwork attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetwork{generation}.ai");
      ActivationNetwork fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetwork{generation}.ai");

      IAI enemy1 = new NeuroAI(ArmyColor.Red, setUpNetwork, draftNetwork,
        exchangeNetwork, attackNetwork, fortifyNetwork);
      IAI enemy2 = new NeuroAI(ArmyColor.Blue, setUpNetwork, draftNetwork,
        exchangeNetwork, attackNetwork, fortifyNetwork);

      NNFitnessFuction fitnessFunc = new NNFitnessFuction(enemy1, enemy2, false, 200);

      double[] weights = new double[1146];

      int index = 0;
      index = Learning.GetWeights(setUpNetwork, index, weights);
      index = Learning.GetWeights(draftNetwork, index, weights);
      index = Learning.GetWeights(exchangeNetwork, index, weights);
      index = Learning.GetWeights(attackNetwork, index, weights);
      Learning.GetWeights(fortifyNetwork, index, weights);

      Console.Clear();
      Console.WriteLine("**** Learn starts ****");

      double[] best = Learning.Learn(100, 60, fitnessFunc, 0.5, 0.01, weights, Console.Out);

      Console.WriteLine("**** Learn ends ****");

      setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
      draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
      exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
      attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
      fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();

      index = 0;
      index = Learning.SetWeights(setUpNetwork, index, best);
      index = Learning.SetWeights(draftNetwork, index, best);
      index = Learning.SetWeights(exchangeNetwork, index, best);
      index = Learning.SetWeights(attackNetwork, index, best);
      Learning.SetWeights(fortifyNetwork, index, best);

      string baseDir = AppDomain.CurrentDomain.BaseDirectory;
      setUpNetwork.Save(baseDir + $"\\AI\\setUpNetwork{nextGeneration}.ai");
      draftNetwork.Save(baseDir + $"\\AI\\draftnetwork{nextGeneration}.ai");
      exchangeNetwork.Save(baseDir + $"\\AI\\exchangeCardNetwork{nextGeneration}.ai");
      attackNetwork.Save(baseDir + $"\\AI\\attackNetwork{nextGeneration}.ai");
      fortifyNetwork.Save(baseDir + $"\\AI\\fortifyNetwork{nextGeneration}.ai");
    }
  }
}