using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro;
using Accord.Genetic;
using Accord.Math.Random;
using System.IO;

namespace Risk.AI.NeuralNetwork.Evolution
{
  public class Learning
  {
    public static int SetWeights(ActivationNetwork network, int index, double[] weights)
    {
      for (int i = 0; i < network.Layers.Length; ++i)
      {
        for (int j = 0; j < network.Layers[i].Neurons.Length; ++j)
        {
          ActivationNeuron n = (ActivationNeuron)network.Layers[i].Neurons[j];

          for (int k = 0; k < n.Weights.Length; ++k)
          {
            n.Weights[k] = weights[index];
            index++;
          }

          n.Threshold = weights[index];
          index++;
        }
      }

      return index;
    }

    public static int GetWeights(ActivationNetwork network, int index, double[] weights)
    {
      for (int i = 0; i < network.Layers.Length; ++i)
      {
        for (int j = 0; j < network.Layers[i].Neurons.Length; ++j)
        {
          ActivationNeuron n = (ActivationNeuron)network.Layers[i].Neurons[j];

          for (int k = 0; k < n.Weights.Length; ++k)
          {
            weights[index] = n.Weights[k];
            index++;
          }

          weights[index] = n.Threshold;
          index++;
        }
      }

      return index;
    }

    public static double[] Learn(int populationSize, int numberOfEpoch, IFitnessFunction fitnessFunc,
      double crossoverRate, double mutationRate, int numberOfWeights, TextWriter output)
    {
      ZigguratUniformOneGenerator ran = new ZigguratUniformOneGenerator();
      DoubleArrayChromosome chromosome = new DoubleArrayChromosome(ran, ran, ran, numberOfWeights);

      return Learn(populationSize, numberOfEpoch, fitnessFunc, crossoverRate, mutationRate, chromosome, output);
    }

    public static double[] Learn(int populationSize, int numberOfEpoch, IFitnessFunction fitnessFunc,
      double crossoverRate, double mutationRate, double[] value, TextWriter output)
    {
      ZigguratUniformOneGenerator ran = new ZigguratUniformOneGenerator();
      DoubleArrayChromosome chromosome = new DoubleArrayChromosome(ran, ran, ran, value);

      return Learn(populationSize, numberOfEpoch, fitnessFunc, crossoverRate, mutationRate, chromosome, output);
    }

    private static double[] Learn(int populationSize, int numberOfEpoch, IFitnessFunction fitnessFunc,
      double crossoverRate, double mutationRate, IChromosome chromosome, TextWriter output)
    {
      EliteSelection elite = new EliteSelection();

      Population population = new Population(populationSize, chromosome, fitnessFunc, elite);

      population.CrossoverRate = crossoverRate;
      population.MutationRate = mutationRate;

      for (int i = 0; i < numberOfEpoch; ++i)
      {
        population.RunEpoch();

        output.WriteLine();
        output.WriteLine();

        Console.WriteLine($"Generation: {i + 1}, Average Fitness: {population.FitnessAvg}");
      }

      return ((DoubleArrayChromosome)population.BestChromosome).Value;
    }
  }
}