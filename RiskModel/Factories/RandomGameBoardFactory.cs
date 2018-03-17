using Risk.Model.Interfacies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.GamePlan;
using Risk.Model.Cards;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  public sealed class RandomGameBoardFactory : IGameBoardFactory
  {
    private const int numberOfAreas = 42;

    public GameBoard CreateGameBoard()
    {
      Random ran = new Random();

      int numberOfRegion = ran.Next(3, 9);

      int[] numberOfAreasInRegion = DistributeAreasIntoRegion(numberOfRegion);

      GameBoard gb = new GameBoard(numberOfAreas, GetUnitsPerRegion(numberOfAreasInRegion), GetPackage());

      GenereteAreas(gb, numberOfAreasInRegion);

      GenereteConnections(gb, numberOfAreasInRegion);

      return gb;
    }

    private IList<RiskCard> GetPackage()
    {
      const int numberWildCards = 2;
      const int numberNormalCards = 42;

      IList<RiskCard> package = new List<RiskCard>();

      for (int i = 0; i < numberWildCards; ++i)
      {
        package.Add(new WildCard());
      }

      Random ran = new Random();
      int numberOfOneType = numberNormalCards / 3;
      int[] numberUnits = new int[] { numberOfOneType, numberOfOneType, numberOfOneType };
      for (int i = 0; i < numberNormalCards; ++i)
      {
        int unitType = ran.Next(3);
        while (numberUnits[unitType] == 0)
        {
          unitType = ran.Next(3);
        }
        package.Add(new NormalCard((UnitType)unitType, i));
      }

      return package;
    }

    private int[] DistributeAreasIntoRegion(int numberOfRegion)
    {
      Random ran = new Random();

      int[] numberOfAreasInRegion = new int[numberOfRegion];

      for (int i = 0; i < numberOfAreasInRegion.Length; ++i)
      {
        numberOfAreasInRegion[i] = 3;
      }

      for (int i = 0; i < numberOfAreas - 3 * numberOfRegion; ++i)
      {
        numberOfAreasInRegion[ran.Next(0, numberOfRegion)]++;
      }

      return numberOfAreasInRegion;
    }

    private int[] GetUnitsPerRegion(int[] numberOfAreasInRegion)
    {
      int[] unitsPerRegion = new int[numberOfAreasInRegion.Length];

      for (int i = 0; i < numberOfAreasInRegion.Length; ++i)
      {
        unitsPerRegion[i] = numberOfAreasInRegion[i] / 2 + 1;
      }

      return unitsPerRegion;
    }

    private void GenereteAreas(GameBoard gb, int[] numberOfAreasInRegion)
    {
      int offset = 0;
      for (int i = 0; i < numberOfAreasInRegion.Length; ++i)
      {
        for (int j = 0; j < numberOfAreasInRegion[i]; ++j)
        {
          gb.Areas[offset + j] = new Area(offset + j, i);
        }
        offset += numberOfAreasInRegion[i];
      }
    }

    private void GenereteConnections(GameBoard gb, int[] numberOfAreasInRegion)
    {
      List<List<int>> borders = new List<List<int>>();
      Random ran = new Random();

      int offset = 0;
      for (int k = 0; k < numberOfAreasInRegion.Length; ++k)
      {
        borders.Add(new List<int>());

        MakeTriangle(gb.Connections, offset, offset + 1, offset + 2);
        for (int j = 0; j < 3; ++j)
        {
          borders[k].Add(offset + j);
        }

        List<int> neighbors = new List<int>();
        neighbors.Add(offset + 1);
        neighbors.Add(offset + 2);
        for (int l = 3; l < numberOfAreasInRegion[k]; ++l)
        {
          int desion = ran.Next(1, 101);
          if (desion > 50)
          {
            int i = ran.Next(0, neighbors.Count - 1);
            int j = ran.Next(i + 1, neighbors.Count);
            neighbors = InnerContraction(gb.Connections, offset, offset + l, neighbors, i, j);
          }
          else
          {
            int i = neighbors.Count;
            int j = ran.Next(1, neighbors.Count);
            neighbors = OuterContraction(gb.Connections, offset, offset + l, neighbors, i, j);
            borders[k].Add(offset + l);
          }
        }
        offset += numberOfAreasInRegion[k];
      }

      for (int i = 0; i < numberOfAreasInRegion.Length - 1; ++i)
      {
        int numberOfCon = ran.Next(1, 4);
        for (int j = 0; j < numberOfCon; ++j)
        {
          int fromArea = ran.Next(0, borders[i].Count);
          int toArea = ran.Next(0, borders[(i + j + 1) % borders.Count].Count);
          MakeEdge(gb.Connections, borders[i][fromArea], borders[(i + j + 1) % borders.Count][toArea]);
        }
      }
    }

    private void MakeTriangle(bool[][] connections, int a, int b, int c)
    {
      MakeEdge(connections, a, b);
      MakeEdge(connections, a, c);
      MakeEdge(connections, b, c);
    }

    private List<int> InnerContraction(bool[][] connections, int root, int newVertex, List<int> neighbors, int i, int j)
    {
      if (i + 1 != j)
      {
        for (int k = i + 1; k < j; ++k)
        {
          RemoveEdge(connections, root, neighbors[k]);
        }
      }

      MakeEdge(connections, root, newVertex);

      for (int k = i; k <= j; ++k)
      {
        MakeEdge(connections, newVertex, neighbors[k]);
      }

      return CheckNeighborsInner(connections, newVertex, neighbors, i, j);
    }

    private List<int> OuterContraction(bool[][] connections, int root, int newVertex, List<int> neighbors, int i, int j)
    {
      for (int k = j; k < neighbors.Count; ++k)
      {
        RemoveEdge(connections, root, neighbors[k]);
      }

      MakeEdge(connections, root, newVertex);
      MakeEdge(connections, newVertex, neighbors[neighbors.Count - 1]);

      for (int k = j - 1; k < neighbors.Count; ++k)
      {
        MakeEdge(connections, newVertex, neighbors[k]);
      }

      return CheckNeighborsOuter(connections, newVertex, neighbors, j);
    }

    private List<int> CheckNeighborsInner(bool[][] connections, int newVetrex, List<int> neighbors, int i, int j)
    {
      List<int> newNeighbors = new List<int>();

      for (int k = 0; k <= i; ++k)
      {
        newNeighbors.Add(neighbors[k]);
      }

      newNeighbors.Add(newVetrex);

      for (int k = j; k < neighbors.Count; ++k)
      {
        newNeighbors.Add(neighbors[k]);
      }

      return newNeighbors;
    }

    private List<int> CheckNeighborsOuter(bool[][] connections, int newVertex, List<int> neighbors, int j)
    {
      List<int> newNeighbors = new List<int>();

      for (int k = 0; k < j; ++k)
      {
        newNeighbors.Add(neighbors[k]);
      }

      newNeighbors.Add(newVertex);

      return newNeighbors;
    }

    private void MakeEdge(bool[][] connections, int x, int y)
    {
      connections[x][y] = true;
      connections[y][x] = true;
    }

    private void RemoveEdge(bool[][] connections, int x, int y)
    {
      connections[x][y] = false;
      connections[y][x] = false;
    }
  }
}