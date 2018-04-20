using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Genetic;
using Accord.Neuro;
using Risk.Model.Enums;

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

    public NNFitnessFuction(IAI enemy1, IAI enemy2, bool isClassic, int numberOfGames)
    {
      _players = new List<IAI>();
      _players.Add(enemy1);
      _players.Add(enemy2);

      _setUpNetwork = NeuralNetworkFactory.CreateSetUpNetwork();
      _draftNetwork = NeuralNetworkFactory.CreateDraftNetwork();
      _exchangeNetwork = NeuralNetworkFactory.CreateExchangeNetwork();
      _attackNetwork = NeuralNetworkFactory.CreateAttackNetwork();
      _fortifyNetwork = NeuralNetworkFactory.CreateFortifyNetwork();

      _battle = new BattleOfAI(isClassic, numberOfGames);
      _numberOfGames = numberOfGames;
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

        _players.Add(new NeuroAI(ArmyColor.Green, _setUpNetwork, _draftNetwork, _exchangeNetwork, _attackNetwork, _fortifyNetwork));

        IDictionary<ArmyColor, double> result = _battle.PlaySimulation(_players);

        _players.RemoveAt(2);

        Console.WriteLine($"Fitness: {result[ArmyColor.Green] / (double)_numberOfGames}");

        return result[ArmyColor.Green] / (double)_numberOfGames;
      }
      else
      {
        throw new ArgumentException("Invalid chromosome!");
      }
    }
  }
}