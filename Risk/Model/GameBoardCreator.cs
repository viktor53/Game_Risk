using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model
{
  class GameBoardCreator
  {
    private const int _areasOfNorthAmerica = 9;
    private const int _areasOfSouthAmerica = 4;
    private const int _areasOfAfrica = 6;
    private const int _areasOfAustralia = 4;
    private const int _areasOfAsia = 12;
    private const int _areasOfEuropa = 7;

    public static GameBoard CreateNormalBoard()
    {
      GameBoard board = new GameBoard(42);


      return board;
    }

    private static int CreateNorthAmerica(GameBoard board, int id)
    {
      // creating areas
      for (int i = id; i < id + _areasOfNorthAmerica; i++)
      {
        board.Areas[i] = new Area(i, Region.NorthAmerica);
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

    private static int CreateSouthAmerica(GameBoard board, int id)
    {
      // creating areas
      for(int i = id; i < id + _areasOfSouthAmerica; i++)
      {
        board.Areas[i] = new Area(i, Region.SouthAmerica);
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

    private static int CreateAfrica(GameBoard board, int id)
    {
      // creating areas
      for(int i = id; i < id + _areasOfAfrica; i++)
      {
        board.Areas[i] = new Area(i, Region.Africa);
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

    private static int CreateAustralia(GameBoard board, int id)
    {
      // creating areas
      for(int i = id; i < id + _areasOfAustralia; i++)
      {
        board.Areas[i] = new Area(i, Region.Australie);
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

    private static int CreateAsia(GameBoard board, int id)
    {
      // creating areas
      for(int i = id; i < id + _areasOfAsia; i++)
      {
        board.Areas[i] = new Area(i, Region.Asia);
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

    private static int CreateEurope(GameBoard board, int id)
    {
      // creating areas
      for(int i = id; i < id + _areasOfEuropa; i++)
      {
        board.Areas[i] = new Area(i, Region.Europa);
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

    private static void CreatingConNorthAmSouthAm(GameBoard board, int idNorth, int idSouth)
    {
      CreateEdge(board, idNorth + _areasOfNorthAmerica - 1, idSouth);
    }

    private static void CreatingConSouthAmAfrica(GameBoard board, int idSouth, int idAf)
    {
      CreateEdge(board, idSouth + 2, idAf);
    }

    private static void CreatingConNorthAmEurope(GameBoard board, int idNorth, int idEur)
    {
      CreateEdge(board, idNorth + 2, idEur);
    }

    private static void CreatingConEuropeAfrica(GameBoard board, int idEur, int idAf)
    {
      CreateEdge(board, idEur + 5, idAf);
      CreateEdge(board, idEur + 6, idAf + 1);
    }

    private static void CreatingConAfricaAsia(GameBoard board, int idAf, int idAs)
    {
      CreateEdge(board, idAf + 1, idAs + 9);
      CreateEdge(board, idAf + 2, idAs + 9);
    }

    private static void CreatingConEuropeAsia(GameBoard board, int idEur, int idAs)
    {
      CreateEdge(board, idEur + 2, idAs + 6);
      CreateEdge(board, idEur + 2, idAs + 7);
      CreateEdge(board, idEur + 2, idAs + 9);
      CreateEdge(board, idEur + 6, idAs + 9);
    }

    private static void CreatingConAsiaAustralia(GameBoard board, int idAs, int idAus)
    {
      CreateEdge(board, idAs + 11, idAus);
    }

    private static void CreateEdge(GameBoard board, int id1, int id2)
    {
      board.Board[id1][id2] = true;
      board.Board[id2][id1] = true;
    }
  }
}
