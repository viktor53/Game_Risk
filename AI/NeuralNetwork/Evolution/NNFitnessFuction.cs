using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Genetic;
using Accord.Neuro;
using Risk.Model.Enums;
using System.IO;

namespace Risk.AI.NeuralNetwork.Evolution
{
  public class NNFitnessFuction : IFitnessFunction
  {
    private ActivationNetwork _setUpNetwork;

    private ActivationNetwork _draftNetwork;

    private ActivationNetwork _exchangeNetwork;

    private ActivationNetwork _attackNetwork;

    private ActivationNetwork _fortifyNetwork;

    private BattleOfAI _battle;

    private int _numberOfGames;

    private List<IAI> _players;

    private TextWriter _output;

    private NNFitnessFuction(IAI enemy1, IAI enemy2, bool isComplexTopology, int numberOfGames, TextWriter output)
    {
      _players = new List<IAI>();
      _players.Add(enemy1);
      _players.Add(enemy2);
      _output = output;

      if (isComplexTopology)
      {
        _setUpNetwork = NeuralNetworkFactory.CreateSetUpNetworkComplexTopology();
        _draftNetwork = NeuralNetworkFactory.CreateDraftNetworkComplexTopology();
        _exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetworkComplexTopology();
        _attackNetwork = NeuralNetworkFactory.CreateAttackNetworkComplexTopology();
        _fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetworkComplexTopology();
      }
      else
      {
        _setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
        _draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
        _exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
        _attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
        _fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();
      }

      _numberOfGames = numberOfGames;
    }

    public NNFitnessFuction(IAI enemy1, IAI enemy2, bool isComplexTopology, bool isClassic, int numberOfGames, TextWriter output) : this(enemy1, enemy2, isComplexTopology, numberOfGames, output)
    {
      _battle = new BattleOfAI(isClassic, numberOfGames);
    }

    public NNFitnessFuction(IAI enemy1, IAI enemy2, bool isComplexTopology, int numberOfAreas, int numberOfGames, TextWriter output) : this(enemy1, enemy2, isComplexTopology, numberOfGames, output)
    {
      _battle = new BattleOfAI(numberOfAreas, numberOfGames);
    }

    public double Evaluate(IChromosome chromosome)
    {
      if (chromosome is DoubleArrayChromosome)
      {
        double[] value = ((DoubleArrayChromosome)chromosome).Value;

        int index = 0;
        index = Learning.SetWeights(_setUpNetwork, index, value);
        index = Learning.SetWeights(_draftNetwork, index, value);
        index = Learning.SetWeights(_exchangeNetwork, index, value);
        index = Learning.SetWeights(_attackNetwork, index, value);
        Learning.SetWeights(_fortifyNetwork, index, value);

        NeuroAI ai = new NeuroAI(ArmyColor.Green, _setUpNetwork, _draftNetwork, _exchangeNetwork, _attackNetwork, _fortifyNetwork);

        _players.Add(ai);

        IDictionary<ArmyColor, double> result = _battle.PlaySimulation(_players);

        _players.Remove(ai);

        _output.Write($"{result[ai.PlayerColor] / (double)_numberOfGames} ");

        Console.WriteLine($"Fitness: {result[ai.PlayerColor] / (double)_numberOfGames}");

        return result[ai.PlayerColor] / (double)_numberOfGames;
      }
      else
      {
        throw new ArgumentException("Invalid chromosome!");
      }
    }
  }
}