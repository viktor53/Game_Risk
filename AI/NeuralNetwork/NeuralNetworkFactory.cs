using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro;

namespace Risk.AI.NeuralNetwork
{
  public static class NeuralNetworkFactory
  {
    public static ActivationNetwork CreateSetUpNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 16, 16, 1);
    }

    public static ActivationNetwork CreateExchangeNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 4, 4, 1);
    }

    public static ActivationNetwork CreateDraftNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 9, 9, 2);
    }

    public static ActivationNetwork CreateAttackNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 17, 17, 2);
    }

    public static ActivationNetwork CreateFortifyNetwork()
    {
      return new ActivationNetwork(new SigmoidFunction(), 18, 18, 2);
    }

    public static ActivationNetwork LoadNetwork(string name)
    {
      return (ActivationNetwork)ActivationNetwork.Load(AppDomain.CurrentDomain.BaseDirectory + "\\AI\\" + name);
    }
  }
}