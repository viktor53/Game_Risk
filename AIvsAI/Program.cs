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
using System.IO;

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
            LearnFromStartSettings();
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

    private static void LearnFromStartSettings()
    {
      Console.Clear();
      Console.WriteLine("**** Learn Settings ****");

      bool isComplex = false;
      int numberOfEpoch = 0;

      Console.Write("NN with complex topology (1 - ano, 0 - ne): ");

      int choice = -1;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        if (choice == 1)
        {
          isComplex = true;
        }
        else
        {
          isComplex = false;
        }
      }
      else
      {
        Console.WriteLine("Not a number!");
        return;
      }

      Console.Write("Number of epoch: ");

      choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        numberOfEpoch = choice;
      }
      else
      {
        Console.WriteLine("Not a number!");
        return;
      }

      Console.Write("name of file with evolution logs: ");

      LearnFromStart(isComplex, numberOfEpoch, Console.ReadLine());
    }

    private static void LearnSettings()
    {
      Console.Clear();
      Console.WriteLine("**** Learn Settings ****");

      bool isComplex = false;
      int numberOfEpoch = 0;
      int gen = 0;
      int nextGen = 0;

      Console.Write("NN with complex topology (1 - ano, 0 - ne): ");

      int choice = -1;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        if (choice == 1)
        {
          isComplex = true;
        }
        else
        {
          isComplex = false;
        }
      }
      else
      {
        Console.WriteLine("Not a number!");
        return;
      }

      Console.Write("Number of epoch: ");

      choice = 0;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        numberOfEpoch = choice;
      }
      else
      {
        Console.WriteLine("Not a number!");
        return;
      }

      Console.Write("Number of NN generation to load: ");

      choice = 0;
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

      Console.Write("name of file with evolution logs: ");

      Learn(isComplex, numberOfEpoch, gen, nextGen, Console.ReadLine());
    }

    private static void BattleMenu()
    {
      Console.Clear();
      Console.WriteLine("**** Battle Menu ****");
      Console.WriteLine("** 1 - Random vs Random vs Random");
      Console.WriteLine("** 2 - MCTS vs Random vs Random");
      Console.WriteLine("** 3 - NN vs Random vs Random");
      Console.WriteLine("** 4 - MCTS-NN vs Random vs Random");
      Console.WriteLine("** 5 - NN vs NN vs NN");
      Console.WriteLine("** 6 - MCTS vs NN vs MCTS-NN");
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
            Battle(GetMCTSNNAndRandom(), "MCTS-NN", "Random agent", "Random agent");
            break;

          case 5:
            Battle(GetNNOnly(), "NN agent 1", "NN agent 2", "NN agent 3");
            break;

          case 6:
            Battle(GetMCTSAndNNAndMCTSNN(), "MCTS agent", "NN agent", "MCTS-NN agent");
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
      BattleOfAI battle = null;

      Console.Write("Classic game (1 - ano, 0 - ne): ");

      int choice = -1;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        if (choice == 1)
        {
          battle = new BattleOfAI(true, 500);
        }
        else
        {
          Console.Write("Number of areas (9 - 42): ");

          if (int.TryParse(Console.ReadLine(), out choice))
          {
            battle = new BattleOfAI(choice, 500);
          }
          else
          {
            throw new ArgumentException("Not a number!");
          }
        }
      }
      else
      {
        throw new ArgumentException("Not a number!");
      }

      Console.Clear();

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
      RandomPlayer green = new RandomPlayer(ArmyColor.Green);
      RandomPlayer red = new RandomPlayer(ArmyColor.Red);
      RandomPlayer blue = new RandomPlayer(ArmyColor.Blue);
      agents.Add(green);
      agents.Add(red);
      agents.Add(blue);

      return agents;
    }

    private static IList<IAI> GetMCTSAndRandom()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(new MCTSAI(ArmyColor.Green, false));
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

    private static IList<IAI> GetMCTSNNAndRandom()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(new MCTSAI(ArmyColor.Green, true));
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

    private static IList<IAI> GetMCTSAndNNAndMCTSNN()
    {
      List<IAI> agents = new List<IAI>();
      agents.Add(new MCTSAI(ArmyColor.Green, false));
      agents.Add(LoadNN(ArmyColor.Red));
      agents.Add(new MCTSAI(ArmyColor.Blue, true));

      return agents;
    }

    private static IAI LoadNN(ArmyColor agentColor)
    {
      Console.Write("NN with complex topology (1 - ano, 0 - ne): ");

      bool isComplex = false;
      string generation;

      int choice = -1;
      if (int.TryParse(Console.ReadLine(), out choice))
      {
        if (choice == 1)
        {
          isComplex = true;
        }
        else
        {
          isComplex = false;
        }
      }
      else
      {
        throw new ArgumentException("Not a number!");
      }

      Console.Write("Number of NN generation to load: ");

      generation = Console.ReadLine();

      ActivationNetwork setUpNetwork;
      ActivationNetwork draftNetwork;
      ActivationNetwork exchangeNetwork;
      ActivationNetwork attackNetwork;
      ActivationNetwork fortifyNetwork;

      if (isComplex)
      {
        if (generation == "-1")
        {
          setUpNetwork = NeuralNetworkFactory.CreateSetUpNetworkComplexTopology();
          draftNetwork = NeuralNetworkFactory.CreateDraftNetworkComplexTopology();
          exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetworkComplexTopology();
          attackNetwork = NeuralNetworkFactory.CreateAttackNetworkComplexTopology();
          fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetworkComplexTopology();
        }
        else
        {
          setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetworkComplex{generation}.ai");
          draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetworkComplex{generation}.ai");
          exchangeNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetworkComplex{generation}.ai");
          attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetworkComplex{generation}.ai");
          fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetworkComplex{generation}.ai");
        }
      }
      else
      {
        if (generation == "-1")
        {
          setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
          draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
          exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
          attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
          fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();
        }
        else
        {
          setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetwork{generation}.ai");
          draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetwork{generation}.ai");
          exchangeNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetwork{generation}.ai");
          attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetwork{generation}.ai");
          fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetwork{generation}.ai");
        }
      }

      return new NeuroAI(agentColor, setUpNetwork, draftNetwork, exchangeNetwork, attackNetwork, fortifyNetwork);
    }

    private static void LearnFromStart(bool isComplex, int numberOfEpoch, string nameOfEvolutionLog)
    {
      ActivationNetwork setUpNetwork;
      ActivationNetwork draftNetwork;
      ActivationNetwork exchangeNetwork;
      ActivationNetwork attackNetwork;
      ActivationNetwork fortifyNetwork;

      int numberOfWeights = 0;

      if (isComplex)
      {
        setUpNetwork = NeuralNetworkFactory.CreateSetUpNetworkComplexTopology();
        draftNetwork = NeuralNetworkFactory.CreateDraftNetworkComplexTopology();
        exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetworkComplexTopology();
        attackNetwork = NeuralNetworkFactory.CreateAttackNetworkComplexTopology();
        fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetworkComplexTopology();

        numberOfWeights = 1944;
      }
      else
      {
        setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
        draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
        exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
        attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
        fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();

        numberOfWeights = 1146;
      }

      IAI enemy1 = new NeuroAI(ArmyColor.Red, setUpNetwork, draftNetwork,
        exchangeNetwork, attackNetwork, fortifyNetwork);
      IAI enemy2 = new NeuroAI(ArmyColor.Blue, setUpNetwork, draftNetwork,
        exchangeNetwork, attackNetwork, fortifyNetwork);

      using (TextWriter output = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\AI\\" + nameOfEvolutionLog))
      {
        NNFitnessFunction fitnessFunc = new NNFitnessFunction(enemy1, enemy2, isComplex, 21, 200, output);

        Console.Clear();
        Console.WriteLine("**** Learn starts ****");

        double[] best = Learning.Learn(100, numberOfEpoch, fitnessFunc, 0.5, 0.01, numberOfWeights, output);

        Console.WriteLine("**** Learn ends ****");

        int index = 0;
        index = Learning.SetWeights(setUpNetwork, index, best);
        index = Learning.SetWeights(draftNetwork, index, best);
        index = Learning.SetWeights(exchangeNetwork, index, best);
        index = Learning.SetWeights(attackNetwork, index, best);
        Learning.SetWeights(fortifyNetwork, index, best);

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        if (isComplex)
        {
          setUpNetwork.Save(baseDir + "\\AI\\setUpNetworkComplex0.ai");
          draftNetwork.Save(baseDir + "\\AI\\draftnetworkComplex0.ai");
          exchangeNetwork.Save(baseDir + "\\AI\\exchangeCardNetworkComplex0.ai");
          attackNetwork.Save(baseDir + "\\AI\\attackNetworkComplex0.ai");
          fortifyNetwork.Save(baseDir + "\\AI\\fortifyNetworkComplex0.ai");
        }
        else
        {
          setUpNetwork.Save(baseDir + "\\AI\\setUpNetwork0.ai");
          draftNetwork.Save(baseDir + "\\AI\\draftnetwork0.ai");
          exchangeNetwork.Save(baseDir + "\\AI\\exchangeCardNetwork0.ai");
          attackNetwork.Save(baseDir + "\\AI\\attackNetwork0.ai");
          fortifyNetwork.Save(baseDir + "\\AI\\fortifyNetwork0.ai");
        }
      }
    }

    private static void Learn(bool isComplex, int numberOfEpoch, int generation, int nextGeneration, string nameOfEvolutionLog)
    {
      ActivationNetwork setUpNetwork;
      ActivationNetwork draftNetwork;
      ActivationNetwork exchangeNetwork;
      ActivationNetwork attackNetwork;
      ActivationNetwork fortifyNetwork;

      int numberOfWeights = 0;

      if (isComplex)
      {
        setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetworkComplex{generation}.ai");
        draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetworkComplex{generation}.ai");
        exchangeNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetworkComplex{generation}.ai");
        attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetworkComplex{generation}.ai");
        fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetworkComplex{generation}.ai");

        numberOfWeights = 1944;
      }
      else
      {
        setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetwork{generation}.ai");
        draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetwork{generation}.ai");
        exchangeNetwork = NeuralNetworkFactory.LoadNetwork($"exchangeCardNetwork{generation}.ai");
        attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetwork{generation}.ai");
        fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetwork{generation}.ai");

        numberOfWeights = 1146;
      }

      IAI enemy1 = new NeuroAI(ArmyColor.Red, setUpNetwork, draftNetwork,
        exchangeNetwork, attackNetwork, fortifyNetwork);
      IAI enemy2 = new NeuroAI(ArmyColor.Blue, setUpNetwork, draftNetwork,
        exchangeNetwork, attackNetwork, fortifyNetwork);

      double[] weights = new double[numberOfWeights];

      int index = 0;
      index = Learning.GetWeights(setUpNetwork, index, weights);
      index = Learning.GetWeights(draftNetwork, index, weights);
      index = Learning.GetWeights(exchangeNetwork, index, weights);
      index = Learning.GetWeights(attackNetwork, index, weights);
      Learning.GetWeights(fortifyNetwork, index, weights);

      using (TextWriter output = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\AI\\" + nameOfEvolutionLog))
      {
        NNFitnessFunction fitnessFunc = new NNFitnessFunction(enemy1, enemy2, isComplex, 21, 200, output);

        Console.Clear();
        Console.WriteLine("**** Learn starts ****");

        double[] best = Learning.Learn(100, numberOfEpoch, fitnessFunc, 0.5, 0.01, weights, output);

        Console.WriteLine("**** Learn ends ****");

        index = 0;
        index = Learning.SetWeights(setUpNetwork, index, best);
        index = Learning.SetWeights(draftNetwork, index, best);
        index = Learning.SetWeights(exchangeNetwork, index, best);
        index = Learning.SetWeights(attackNetwork, index, best);
        Learning.SetWeights(fortifyNetwork, index, best);

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        if (isComplex)
        {
          setUpNetwork.Save(baseDir + $"\\AI\\setUpNetworkComplex{nextGeneration}.ai");
          draftNetwork.Save(baseDir + $"\\AI\\draftnetworkComplex{nextGeneration}.ai");
          exchangeNetwork.Save(baseDir + $"\\AI\\exchangeCardNetworkComplex{nextGeneration}.ai");
          attackNetwork.Save(baseDir + $"\\AI\\attackNetworkComplex{nextGeneration}.ai");
          fortifyNetwork.Save(baseDir + $"\\AI\\fortifyNetworkComplex{nextGeneration}.ai");
        }
        else
        {
          setUpNetwork.Save(baseDir + $"\\AI\\setUpNetwork{nextGeneration}.ai");
          draftNetwork.Save(baseDir + $"\\AI\\draftnetwork{nextGeneration}.ai");
          exchangeNetwork.Save(baseDir + $"\\AI\\exchangeCardNetwork{nextGeneration}.ai");
          attackNetwork.Save(baseDir + $"\\AI\\attackNetwork{nextGeneration}.ai");
          fortifyNetwork.Save(baseDir + $"\\AI\\fortifyNetwork{nextGeneration}.ai");
        }
      }
    }
  }
}