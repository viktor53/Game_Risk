using System;
using System.Collections.Generic;
using Risk.Model.GamePlan;
using Risk.Model.Enums;
using Risk.Model.Cards;

namespace Risk.Model.Factories
{
  /// <summary>
  /// Factory for creating classic game board.
  /// </summary>
  public sealed class ClassicGameBoardFactory : IGameBoardFactory
  {
    private const int _areasOfNorthAmerica = 9;
    private const int _areasOfSouthAmerica = 4;
    private const int _areasOfAfrica = 6;
    private const int _areasOfAustralia = 4;
    private const int _areasOfAsia = 12;
    private const int _areasOfEuropa = 7;

    private const int _numberWildCards = 2;
    private const int _numberNormalCards = 42;

    /// <summary>
    /// Creates classic game board.
    /// </summary>
    /// <returns>new classic game board</returns>
    public GameBoard CreateGameBoard()
    {
      GameBoard board = new GameBoard(42, GetNumberUnitsForRegion(), GetPackage());

      int id1, id2, id3;

      id1 = CreateNorthAmerica(board, 0);

      id2 = CreateSouthAmerica(board, id1);

      CreatingConNorthAmSouthAm(board, 0, id1);

      id3 = CreateAfrica(board, id2);

      CreatingConSouthAmAfrica(board, id1, id2);

      id1 = CreateEurope(board, id3);

      CreatingConNorthAmEurope(board, 0, id3);

      CreatingConEuropeAfrica(board, id3, id2);

      int id4 = CreateAsia(board, id1);

      CreatingConEuropeAsia(board, id3, id1);

      CreatingConAfricaAsia(board, id2, id1);

      CreateAustralia(board, id4);

      CreatingConAsiaAustralia(board, id1, id4);

      return board;
    }

    /// <summary>
    /// Creates new game board.
    /// </summary>
    /// <param name="numberOfAreas">number of areas on game board</param>
    /// <returns>new game board</returns>
    public GameBoard CreateGameBoard(int numberOfAreas)
    {
      throw new NotSupportedException("Classic game has 42 areas not more and not less!");
    }

    /// <summary>
    /// Gets free units fro the region with ID as index.
    /// </summary>
    /// <returns>free units for the region</returns>
    private int[] GetNumberUnitsForRegion()
    {
      return new int[] { 2, 5, 2, 3, 7, 5 };
    }

    /// <summary>
    /// Creates package of risk cards.
    /// </summary>
    /// <returns>new package of risk cards</returns>
    private IList<RiskCard> GetPackage()
    {
      IList<RiskCard> package = new List<RiskCard>();

      for (int i = 0; i < _numberWildCards; ++i)
      {
        package.Add(new WildCard());
      }

      Random ran = new Random();
      int numberOfOneType = _numberNormalCards / 3;
      int[] numberUnits = new int[] { numberOfOneType, numberOfOneType, numberOfOneType };
      for (int i = 0; i < _numberNormalCards; ++i)
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
    /// Creates North America region.
    /// </summary>
    /// <param name="board">game board where region is creted</param>
    /// <param name="id">start id of areas</param>
    /// <returns>next id without area</returns>
    private int CreateNorthAmerica(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfNorthAmerica; i++)
      {
        board.Areas[i] = new Area((byte)i, (int)Region.NorthAmerica);
      }

      // creating edges between Aliaska and its neighbours
      CreateEdge(board, id, id + 1);
      CreateEdge(board, id, id + 3);

      // creating edges between Northen Territory and its neighbours
      CreateEdge(board, id + 1, id + 2);
      CreateEdge(board, id + 1, id + 3);
      CreateEdge(board, id + 1, id + 4);

      // creating edges between Greenland and its neighbours
      CreateEdge(board, id + 2, id + 5);

      // creating edges between Albaria and its neighbours
      CreateEdge(board, id + 3, id + 4);
      CreateEdge(board, id + 3, id + 6);

      // creating edges between Ontario and its neighbours
      CreateEdge(board, id + 4, id + 5);
      CreateEdge(board, id + 4, id + 6);
      CreateEdge(board, id + 4, id + 7);

      // creating edges between Eastern Canada and its neighbours
      CreateEdge(board, id + 5, id + 7);

      // creating edges between Western United States and its neighbours
      CreateEdge(board, id + 6, id + 7);
      CreateEdge(board, id + 6, id + 8);

      // creating edges between Eastern United States and its neighbours
      CreateEdge(board, id + 6, id + 8);

      // last area has all edges between its neighbours in this region

      return id + _areasOfNorthAmerica;
    }

    /// <summary>
    /// Creates South America region.
    /// </summary>
    /// <param name="board">game board where region is creted</param>
    /// <param name="id">start id of areas</param>
    /// <returns>next id without area</returns>
    private static int CreateSouthAmerica(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfSouthAmerica; i++)
      {
        board.Areas[i] = new Area((byte)i, (int)Region.SouthAmerica);
      }

      // creating edges between Venezuela and its neighbours
      CreateEdge(board, id, id + 1);
      CreateEdge(board, id, id + 2);

      // creating edges between Peru and its neighbours
      CreateEdge(board, id + 1, id + 2);
      CreateEdge(board, id + 1, id + 3);

      // creating edges between Brazil and its neighbours
      CreateEdge(board, id + 2, id + 3);

      // last area has all edges between its neighbours in this region

      return id + _areasOfSouthAmerica;
    }

    /// <summary>
    /// Creates Africa region.
    /// </summary>
    /// <param name="board">game board where region is creted</param>
    /// <param name="id">start id of areas</param>
    /// <returns>next id without area</returns>
    private static int CreateAfrica(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfAfrica; i++)
      {
        board.Areas[i] = new Area((byte)i, (int)Region.Africa);
      }

      // creating edges between North Africa and its neighbours
      CreateEdge(board, id, id + 1);
      CreateEdge(board, id, id + 2);
      CreateEdge(board, id, id + 3);

      // creating edges between Egypt and its neighbours
      CreateEdge(board, id + 1, id + 2);

      // creating edges between East Africa and its neighbours
      CreateEdge(board, id + 2, id + 3);
      CreateEdge(board, id + 2, id + 5);

      // creating edges between Central Africa and its neighbours
      CreateEdge(board, id + 3, id + 4);

      // creating edges between South Africa and its neighbours
      CreateEdge(board, id + 4, id + 5);

      // last area has all edges between its neighbours in this region

      return id + _areasOfAfrica;
    }

    /// <summary>
    /// Creates Australia region.
    /// </summary>
    /// <param name="board">game board where region is creted</param>
    /// <param name="id">start id of areas</param>
    /// <returns>next id without area</returns>
    private static int CreateAustralia(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfAustralia; i++)
      {
        board.Areas[i] = new Area((byte)i, (int)Region.Australie);
      }

      // creating edges between Indonesia and its neighbours
      CreateEdge(board, id, id + 1);
      CreateEdge(board, id, id + 2);

      // creating edges between New Guinea and its neighbours
      CreateEdge(board, id + 1, id + 2);
      CreateEdge(board, id + 1, id + 3);

      // creating edges between Western Australia and its neighbours
      CreateEdge(board, id + 2, id + 3);

      // last area has all edges between its neighbours in this region

      return id + _areasOfAustralia;
    }

    /// <summary>
    /// Creates Asia region.
    /// </summary>
    /// <param name="board">game board where region is creted</param>
    /// <param name="id">start id of areas</param>
    /// <returns>next id without area</returns>
    private static int CreateAsia(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfAsia; i++)
      {
        board.Areas[i] = new Area((byte)i, (int)Region.Asia);
      }

      // creating edges between Ural and its neighbours
      CreateEdge(board, id, id + 1);
      CreateEdge(board, id, id + 7);
      CreateEdge(board, id, id + 8);

      // creating edges between Siberia and its neighbours
      CreateEdge(board, id + 1, id + 2);
      CreateEdge(board, id + 1, id + 4);
      CreateEdge(board, id + 1, id + 5);

      // creating edges between Yakutsk and its neighbours
      CreateEdge(board, id + 2, id + 3);
      CreateEdge(board, id + 2, id + 4);

      // creating edges between Kamchatsk and its neighbours
      CreateEdge(board, id + 3, id + 4);
      CreateEdge(board, id + 3, id + 6);

      // creating edges between Irkutsk and its neighbours
      CreateEdge(board, id + 4, id + 5);

      // creating edges between Mongolia and its neighbours
      CreateEdge(board, id + 5, id + 6);

      // creating edges between Afganistan and its neighbours
      CreateEdge(board, id + 7, id + 8);
      CreateEdge(board, id + 7, id + 9);
      CreateEdge(board, id + 7, id + 10);

      // creating edges between China and its neighbours
      CreateEdge(board, id + 8, id + 10);
      CreateEdge(board, id + 8, id + 11);

      // creating edges between Middle East and its neighbours
      CreateEdge(board, id + 9, id + 10);

      // creating edges between Idnia and its neighbours
      CreateEdge(board, id + 10, id + 11);

      // last two areas have all edges between its neighbours in this region

      return id + _areasOfAsia;
    }

    /// <summary>
    /// Creats Europe region.
    /// </summary>
    /// <param name="board">game board where region is creted</param>
    /// <param name="id">start id of areas</param>
    /// <returns>next id without area</returns>
    private static int CreateEurope(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfEuropa; i++)
      {
        board.Areas[i] = new Area((byte)i, (int)Region.Europa);
      }

      // creating edges between Island and its neighbours
      CreateEdge(board, id, id + 1);
      CreateEdge(board, id, id + 3);

      // creating edges between Scandinavia and its neighbours
      CreateEdge(board, id + 1, id + 2);
      CreateEdge(board, id + 1, id + 3);
      CreateEdge(board, id + 1, id + 4);

      // creating edges between Russia and its neighbours
      CreateEdge(board, id + 2, id + 4);
      CreateEdge(board, id + 2, id + 6);

      // creating edges between Great Britain and its neighbours
      CreateEdge(board, id + 3, id + 4);

      // creating edges between Northen Europe and its neighbours
      CreateEdge(board, id + 4, id + 5);
      CreateEdge(board, id + 4, id + 6);

      // creating edges between Western Euorope and its neighbours
      CreateEdge(board, id + 5, id + 6);

      return id + _areasOfEuropa;
    }

    /// <summary>
    /// Creates connections between North America and South America.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idNorth">first ID of North America</param>
    /// <param name="idSouth">firt ID of South America</param>
    private static void CreatingConNorthAmSouthAm(GameBoard board, int idNorth, int idSouth)
    {
      CreateEdge(board, idNorth + _areasOfNorthAmerica - 1, idSouth);
    }

    /// <summary>
    /// Creates connections between South America and Africa.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idSouth">first ID of South America</param>
    /// <param name="idAf">first ID of Africa</param>
    private static void CreatingConSouthAmAfrica(GameBoard board, int idSouth, int idAf)
    {
      CreateEdge(board, idSouth + 2, idAf);
    }

    /// <summary>
    /// Creates connections between North America and Europe.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idNorth">first ID of North America</param>
    /// <param name="idEur">first ID of Europe</param>
    private static void CreatingConNorthAmEurope(GameBoard board, int idNorth, int idEur)
    {
      CreateEdge(board, idNorth + 2, idEur);
    }

    /// <summary>
    /// Creates connectios between Europe and Africa.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idEur">first ID of Europe</param>
    /// <param name="idAf">first ID of Africa</param>
    private static void CreatingConEuropeAfrica(GameBoard board, int idEur, int idAf)
    {
      CreateEdge(board, idEur + 5, idAf);
      CreateEdge(board, idEur + 6, idAf + 1);
    }

    /// <summary>
    /// Creates connections between Africa and Asia.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idAf">first ID of Africa</param>
    /// <param name="idAs">first ID of Asia</param>
    private static void CreatingConAfricaAsia(GameBoard board, int idAf, int idAs)
    {
      CreateEdge(board, idAf + 1, idAs + 9);
      CreateEdge(board, idAf + 2, idAs + 9);
    }

    /// <summary>
    /// Creates connections between Europe and Asia.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idEur">first ID of Europe</param>
    /// <param name="idAs">first ID of Asia</param>
    private static void CreatingConEuropeAsia(GameBoard board, int idEur, int idAs)
    {
      CreateEdge(board, idEur + 2, idAs + 6);
      CreateEdge(board, idEur + 2, idAs + 7);
      CreateEdge(board, idEur + 2, idAs + 9);
      CreateEdge(board, idEur + 6, idAs + 9);
    }

    /// <summary>
    /// Creates connections between Asia and Australia.
    /// </summary>
    /// <param name="board">game board where connections are created</param>
    /// <param name="idAs">first ID of Asia</param>
    /// <param name="idAus">first ID of Australia</param>
    private static void CreatingConAsiaAustralia(GameBoard board, int idAs, int idAus)
    {
      CreateEdge(board, idAs + 11, idAus);
    }

    /// <summary>
    /// Creates edge between areas A,B.
    /// </summary>
    /// <param name="board">game board where edge is created</param>
    /// <param name="id1">area A</param>
    /// <param name="id2">area B</param>
    private static void CreateEdge(GameBoard board, int id1, int id2)
    {
      board.Connections[id1][id2] = true;
      board.Connections[id2][id1] = true;
    }
  }
}