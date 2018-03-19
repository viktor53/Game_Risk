using System;
using System.Collections.Generic;
using Risk.Model.GamePlan;
using Risk.Model.Cards;
using Risk.Model.Enums;

namespace Risk.Model.Factories
{
  /// <summary>
  /// Factory for creating random game board.
  /// </summary>
  public sealed class RandomGameBoardFactory : IGameBoardFactory
  {
    private const int _numberOfAreas = 42;

    /// <summary>
    /// Creates random game board.
    /// </summary>
    /// <returns>new random game board</returns>
    public GameBoard CreateGameBoard()
    {
      Random ran = new Random();

      int numberOfRegion = ran.Next(3, 9);

      int[] numberOfAreasInRegion = DistributeAreasIntoRegion(numberOfRegion);

      GameBoard gb = new GameBoard(_numberOfAreas, GetUnitsPerRegion(numberOfAreasInRegion), GetPackage());

      GenereteAreas(gb, numberOfAreasInRegion);

      GenereteConnections(gb, numberOfAreasInRegion);

      return gb;
    }

    /// <summary>
    /// Creates package of risk cards.
    /// </summary>
    /// <returns>new package of risk cards</returns>
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

    /// <summary>
    /// Randomly distribute areas into region, but does not creat areas only count them.
    /// </summary>
    /// <param name="numberOfRegion">number of region</param>
    /// <returns>number of areas in the region with ID as index</returns>
    private int[] DistributeAreasIntoRegion(int numberOfRegion)
    {
      Random ran = new Random();

      int[] numberOfAreasInRegion = new int[numberOfRegion];

      for (int i = 0; i < numberOfAreasInRegion.Length; ++i)
      {
        numberOfAreasInRegion[i] = 3;
      }

      for (int i = 0; i < _numberOfAreas - 3 * numberOfRegion; ++i)
      {
        numberOfAreasInRegion[ran.Next(0, numberOfRegion)]++;
      }

      return numberOfAreasInRegion;
    }

    /// <summary>
    /// Counts free units for the region with ID as index.
    /// </summary>
    /// <param name="numberOfAreasInRegion">number of areas in the region with ID as index</param>
    /// <returns>free units for the region</returns>
    private int[] GetUnitsPerRegion(int[] numberOfAreasInRegion)
    {
      int[] unitsPerRegion = new int[numberOfAreasInRegion.Length];

      for (int i = 0; i < numberOfAreasInRegion.Length; ++i)
      {
        unitsPerRegion[i] = numberOfAreasInRegion[i] / 2 + 1;
      }

      return unitsPerRegion;
    }

    /// <summary>
    /// Generates areas into regions depending on number of areas in the region.
    /// </summary>
    /// <param name="gb">game board where areas are created</param>
    /// <param name="numberOfAreasInRegion">number of areas in region</param>
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

    /// <summary>
    /// Generetes connections between areas using algorithm for generating planar tri-connected graph.
    /// </summary>
    /// <param name="gb">game board where connections are created</param>
    /// <param name="numberOfAreasInRegion">number of areas in region</param>
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

    /// <summary>
    /// Creates triangle with verteces A,B,C.
    /// </summary>
    /// <param name="connections">connections where triangle is created</param>
    /// <param name="a">vertex A</param>
    /// <param name="b">vertex B</param>
    /// <param name="c">vertex C</param>
    private void MakeTriangle(bool[][] connections, int a, int b, int c)
    {
      MakeEdge(connections, a, b);
      MakeEdge(connections, a, c);
      MakeEdge(connections, b, c);
    }

    /// <summary>
    /// Generates planar tri-connected graph with one more vertex using inner contraction.
    /// </summary>
    /// <param name="connections">connections where graph is created</param>
    /// <param name="root">root vertex</param>
    /// <param name="newVertex">new vertex</param>
    /// <param name="neighbors">neighbors of root vertex in counterclockwise order</param>
    /// <param name="i">i-th vertex in neighbors where 0 =&lt i &lt j</param>
    /// <param name="j">j-th vertex in neighbors where i &lt j &lt d(root)</param>
    /// <returns></returns>
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

      return CheckNeighborsInner(newVertex, neighbors, i, j);
    }

    /// <summary>
    /// Generates planar tri-connected graph with one more vertex using outer contraction.
    /// </summary>
    /// <param name="connections">connections where graph is created</param>
    /// <param name="root">root vertex</param>
    /// <param name="newVertex">new vertex</param>
    /// <param name="neighbors">neighbors of root vertex in counterclockwise order</param>
    /// <param name="i">i-th vertex in neighbors where i = d(root)</param>
    /// <param name="j">j-th vertex in neighbors where 1 &lt j =&lt i</param>
    /// <returns></returns>
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

      return CheckNeighborsOuter(newVertex, neighbors, j);
    }

    /// <summary>
    /// Removes from neighbors verteces that now are not neighbors and adds new neighbors after inner contraction.
    /// </summary>
    /// <param name="newVetrex">new vertex that was added</param>
    /// <param name="neighbors">neighbors of root vertex</param>
    /// <param name="i">parametr i in inner contraction</param>
    /// <param name="j">parametr j in inner contraction</param>
    /// <returns></returns>
    private List<int> CheckNeighborsInner(int newVetrex, List<int> neighbors, int i, int j)
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

    /// <summary>
    /// Removes from neighbors verteces that now are not neighbors and adds new neighbors after outer contraction.
    /// </summary>
    /// <param name="newVertex">new vertex that was added</param>
    /// <param name="neighbors">neighbors of root vertex</param>
    /// <param name="j">parametr j in outer contraction</param>
    /// <returns></returns>
    private List<int> CheckNeighborsOuter(int newVertex, List<int> neighbors, int j)
    {
      List<int> newNeighbors = new List<int>();

      for (int k = 0; k < j; ++k)
      {
        newNeighbors.Add(neighbors[k]);
      }

      newNeighbors.Add(newVertex);

      return newNeighbors;
    }

    /// <summary>
    /// Creates edge between verteces X,Y.
    /// </summary>
    /// <param name="connections">connections where edge is created</param>
    /// <param name="x">vertex X</param>
    /// <param name="y">vertex Y</param>
    private void MakeEdge(bool[][] connections, int x, int y)
    {
      connections[x][y] = true;
      connections[y][x] = true;
    }

    /// <summary>
    /// Removes edge between verteces X,Y.
    /// </summary>
    /// <param name="connections">connections where edge is created</param>
    /// <param name="x">vertex X</param>
    /// <param name="y">vertex Y</param>
    private void RemoveEdge(bool[][] connections, int x, int y)
    {
      connections[x][y] = false;
      connections[y][x] = false;
    }
  }
}