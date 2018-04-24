using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Neuro;
using Risk.AI.NeuralNetwork;
using Risk.Model.GameCore.Moves;
using Risk.Model.Enums;
using Risk.Model.GamePlan;

namespace Risk.AI.MCTS
{
  internal class NeuroHeuristic
  {
    private ActivationNetwork _setUpNetwork;

    private ActivationNetwork _draftNetwork;

    private ActivationNetwork _attackNetwork;

    private ActivationNetwork _fortifyNetwork;

    private IDictionary<int, RegionInformation> _regionsInfo;

    public NeuroHeuristic(bool isComplex, int generation, IDictionary<int, RegionInformation> regionsInfo)
    {
      if (isComplex)
      {
        _setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetworkComplex{generation}.ai");
        _draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetworkComplex{generation}.ai");
        _attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetworkComplex{generation}.ai");
        _fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetworkComplex{generation}.ai");
      }
      else
      {
        _setUpNetwork = NeuralNetworkFactory.LoadNetwork($"setUpNetwork{generation}.ai");
        _draftNetwork = NeuralNetworkFactory.LoadNetwork($"draftNetwork{generation}.ai");
        _attackNetwork = NeuralNetworkFactory.LoadNetwork($"attackNetwork{generation}.ai");
        _fortifyNetwork = NeuralNetworkFactory.LoadNetwork($"fortifyNetwork{generation}.ai");
      }

      _regionsInfo = regionsInfo;
    }

    public IList<SetUp> GetSetUpPossibilities(ArmyColor aiColor, IList<Area> areas, IList<IList<bool>> connections)
    {
      IList<SetUp> possibilites = new List<SetUp>();

      IList<Area> setUpAreas = Helper.GetUnoccupiedAreas(areas);

      if (setUpAreas.Count == 0)
      {
        setUpAreas = Helper.GetMyAreas(areas, aiColor);
      }

      double[] input = new double[16];

      for (int i = 0; i < setUpAreas.Count; ++i)
      {
        InputBuilder.PrepareSetUpInput(aiColor, setUpAreas[i], areas, connections, _regionsInfo, input);

        double result = _setUpNetwork.Compute(input)[0];

        if (result > 0.5)
        {
          possibilites.Add(new SetUp(aiColor, setUpAreas[i].ID));
        }
      }

      return possibilites;
    }

    public IList<Draft> GetDraftPossibilities(ArmyColor aiColor, int freeUnit, IList<Area> areas, IList<IList<bool>> connections)
    {
      IList<Draft> possibilites = new List<Draft>();

      IList<Area> draftAreas = Helper.GetMyAreas(areas, aiColor);

      double[] input = new double[9];

      for (int i = 0; i < draftAreas.Count; ++i)
      {
        InputBuilder.PrepareDraftInput(aiColor, draftAreas[i], areas, connections, input);

        double[] result = _draftNetwork.Compute(input);

        if (result[0] > 0.5)
        {
          int resultArmy = (int)Math.Round(result[1] * freeUnit);
          if (resultArmy == 0)
          {
            resultArmy = 1;
          }

          possibilites.Add(new Draft(aiColor, draftAreas[i].ID, resultArmy));
        }
      }

      return possibilites;
    }

    public int GetAttackSize(ArmyColor aiColor, Area attacker, Area defender, IList<Area> areas, IList<IList<bool>> connections)
    {
      double[] input = new double[17];

      InputBuilder.PrepareAttackFromInput(aiColor, attacker, areas, connections, input);
      InputBuilder.PrepareAttackToInput(aiColor, defender, areas, _regionsInfo, input);

      double[] result = _attackNetwork.Compute(input);

      if (result[0] > 0.5)
      {
        return InputBuilder.GetAttackSize(result[1], attacker.SizeOfArmy);
      }
      else
      {
        return 0;
      }
    }

    public int GetNumberOfArmiesToMove(ArmyColor aiColor, Area attacker, Area defender, int attackSize, IList<Area> areas, IList<IList<bool>> connections)
    {
      double[] input = new double[17];

      InputBuilder.PrepareAttackFromInput(aiColor, attacker, areas, connections, input);
      InputBuilder.PrepareAttackToInput(aiColor, defender, areas, _regionsInfo, input);

      double result = _attackNetwork.Compute(input)[1];

      return (int)Math.Round((attacker.SizeOfArmy - attackSize - 1) * result) + attackSize;
    }

    public IList<Fortify> GetFortifyPossibilities(ArmyColor aiColor, IList<Area> areas, IList<IList<bool>> connections)
    {
      IList<Fortify> possibilities = new List<Fortify>();

      IList<Area> from = Helper.WhoCanFortify(areas, connections, aiColor);

      double[] input = new double[18];

      for (int i = 0; i < from.Count; ++i)
      {
        InputBuilder.PrepareFortifyFromInput(aiColor, from[i], areas, connections, input);

        IList<Area> where = Helper.WhereCanFortify(from[i], areas, connections, aiColor);
        for (int j = 0; j < where.Count; ++j)
        {
          InputBuilder.PrepareFortifyToInput(aiColor, where[j], areas, connections, input);

          double[] result = _fortifyNetwork.Compute(input);

          if (result[0] > 0.5)
          {
            int armyToMove = (int)Math.Round(result[1] * (from[i].SizeOfArmy - 1));

            if (armyToMove == 0)
            {
              armyToMove = 1;
            }

            possibilities.Add(new Fortify(aiColor, from[i].ID, where[j].ID, armyToMove));
          }
        }
      }

      return possibilities;
    }
  }
}