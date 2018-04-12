using Risk.Model.Enums;
using Risk.Model.GameCore;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal class MonteCarloTreeSearch
  {
    private int _numberOfPlayers;

    private IList<IPlayer> _randomPlayers;

    public MonteCarloTreeSearch(int numberOfPlayers, IList<ArmyColor> players)
    {
      _numberOfPlayers = numberOfPlayers;
      _randomPlayers = new List<IPlayer>();
      foreach (var player in players)
      {
        _randomPlayers.Add(new RandomPlayer(player));
      }
    }

    public Moves GetNextMove(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, int currentPlayer, Phase currentPhase)
    {
      State rootState = new State((GameBoard)gameBoard.Clone(), _randomPlayers, State.GetPlayersInfoClone(playersInfo), currentPlayer, currentPhase);
      Node root = new Node(null, rootState);

      ExpandeNode(root);

      for (int i = 0; i < 1000; ++i)
      {
        Node node = SelectNode(root);
        if (node.State.Status != StatusOfGame.INPROGRESS)
        {
          BackPropagation(node, (int)node.State.Status);
        }
        else
        {
          ExpandeNode(node);
          int winner = Simulation(node);
          BackPropagation(node, winner);
        }
      }

      return GetTheBestMove(root);
    }

    private Node SelectNode(Node root)
    {
      Node node = root;
      while (node.Children.Count != 0)
      {
        node = UCT.GetBestNodeUsingUCT(node);
      }

      return node;
    }

    private void BackPropagation(Node node, int winner)
    {
      while (node != null)
      {
        node.Visited++;
        if ((winner + 1) % _numberOfPlayers == node.State.CurrentPlayer)
        {
          node.Wins++;
        }
        node = node.Parent;
      }
    }

    private void ExpandeNode(Node node)
    {
      var posibilities = node.State.GetAllPosibilities();

      if (posibilities != null)
      {
        foreach (var posible in posibilities)
        {
          Node newNode = new Node(node, posible);
          node.Children.Add(newNode);
        }
      }
    }

    private int Simulation(Node node)
    {
      return node.State.Simulate();
    }

    private Moves GetTheBestMove(Node root)
    {
      Node node = null;
      int theBest = -1;

      foreach (var child in root.Children)
      {
        if (theBest < child.Wins)
        {
          theBest = child.Wins;
          node = child;
        }
      }

      return node.State.Moves;
    }
  }
}