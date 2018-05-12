using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro;

namespace Risk.AI.NeuralNetwork
{
  /// <summary>
  /// Provides methods for creating and loading neural networks.
  /// </summary>
  public static class NeuralNetworkFactory
  {
    public static ActivationNetwork CreateSetUpNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 16, 16, 1);
    }

    public static ActivationNetwork CreateSetUpNetworkComplexTopology()
    {
      return new ActivationNetwork(new SigmoidFunction(), 16, 16, 9, 7, 1);
      //return new ActivationNetwork(new SigmoidFunction(), 16, 9, 7, 1);
    }

    public static ActivationNetwork CreateExchangeNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 4, 4, 1);
    }

    public static ActivationNetwork CreateExchangeNetworkComplexTopology()
    {
      return new ActivationNetwork(new SigmoidFunction(), 4, 4, 2, 2, 1);
      //return new ActivationNetwork(new SigmoidFunction(), 4, 2, 2, 1);
    }

    public static ActivationNetwork CreateDraftNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 9, 9, 2);
    }

    public static ActivationNetwork CreateDraftNetworkComplexTopology()
    {
      return new ActivationNetwork(new SigmoidFunction(), 9, 9, 9, 2);
      //return new ActivationNetwork(new SigmoidFunction(), 9, 9, 2);
    }

    public static ActivationNetwork CreateAttackNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 17, 17, 2);
    }

    public static ActivationNetwork CreateAttackNetworkComplexTopology()
    {
      return new ActivationNetwork(new SigmoidFunction(), 17, 17, 10, 7, 2);
      //return new ActivationNetwork(new SigmoidFunction(), 17, 10, 7, 2);
    }

    public static ActivationNetwork CreateFortifyNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 18, 18, 2);
    }

    public static ActivationNetwork CreateFortifyNetworkComplexTopology()
    {
      return new ActivationNetwork(new SigmoidFunction(), 18, 18, 9, 9, 2);
      //return new ActivationNetwork(new SigmoidFunction(), 18, 9, 9, 2);
    }

    public static ActivationNetwork LoadNetwork(string name)
    {
      return (ActivationNetwork)ActivationNetwork.Load(AppDomain.CurrentDomain.BaseDirectory + "\\AI\\" + name);
    }
  }
}