using Risk.Model.GameCore.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal class Moves
  {
    public SetUp SetUpMove { get; set; }

    public IList<ExchangeCard> CardMoves { get; private set; }

    public IList<Draft> DraftMoves { get; private set; }

    public IList<Attack> AttackMoves { get; private set; }

    public IList<Capture> CaptureMoves { get; private set; }

    public Fortify FortifyMove { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Moves()
    {
      CardMoves = new List<ExchangeCard>();
      DraftMoves = new List<Draft>();
      AttackMoves = new List<Attack>();
      CaptureMoves = new List<Capture>();
      FortifyMove = null;
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="moves">Moves</param>
    public Moves(Moves moves)
    {
      SetUpMove = moves.SetUpMove;

      var cardMoves = new List<ExchangeCard>();
      cardMoves.AddRange(moves.CardMoves);
      CardMoves = cardMoves;

      var draftMoves = new List<Draft>();
      draftMoves.AddRange(moves.DraftMoves);
      DraftMoves = draftMoves;

      var attackMoves = new List<Attack>();
      attackMoves.AddRange(moves.AttackMoves);
      AttackMoves = attackMoves;

      var captureMoves = new List<Capture>();
      captureMoves.AddRange(moves.CaptureMoves);
      CaptureMoves = captureMoves;

      FortifyMove = moves.FortifyMove;
    }
  }
}