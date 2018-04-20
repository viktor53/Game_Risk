using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.NeuralNetwork
{
  internal class RegionInformation
  {
    public double BonusForRegion { get; private set; }

    public int NumberOfAreas { get; private set; }

    public double NumberOfAreasForOneArmy { get; private set; }

    public double NumberOfDefendArmies { get; private set; }

    public double DefendRate { get; private set; }

    public RegionInformation(double bonus, int numberOfAreas, double numberOfAreasForOnArmy, double numberOfDefendArmies, double defendRate)
    {
      BonusForRegion = bonus;
      NumberOfAreas = numberOfAreas;
      NumberOfAreasForOneArmy = numberOfDefendArmies;
      NumberOfDefendArmies = numberOfDefendArmies;
      DefendRate = defendRate;
    }
  }
}