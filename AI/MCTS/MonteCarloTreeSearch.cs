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

    private Random _ran;

    public MonteCarloTreeSearch(int numberOfPlayers, IList<ArmyColor> players)
    {
      _numberOfPlayers = numberOfPlayers;
      _randomPlayers = new List<IPlayer>();
      foreach (var player in players)
      {
        _randomPlayers.Add(new RandomPlayer(player));
      }
      _ran = new Random();
    }

    public Moves GetNextMove(GameBoard gameBoard, IDictionary<ArmyColor, Game.PlayerInfo> playersInfo, int currentPlayer, Phase currentPhase)
    {
      State rootState = new State((GameBoard)gameBoard.Clone(), _randomPlayers, State.GetPlayersInfoClone(playersInfo), currentPlayer, currentPhase);
      rootState.Status = StatusOfGame.ROOT;

      Node root = new Node(null, rootState);

      for (int i = 0; i < 1000; ++i)
      {
        Node node = SelectNode(root);

        if (node.State.Status != StatusOfGame.INPROGRESS)
        {
          ExpandeNode(node);
          if (node.Children.Count != 0)
          {
            node = node.Children[0];
          }
        }

        int winner = Simulation(node);

        BackPropagation(node, winner);
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

    private Node SelectRandomChild(Node node)
    {
      Node child = null;

      for (int i = 0; i < node.Children.Count / 2; ++i)
      {
        child = node.Children[_ran.Next(node.Children.Count)];
        if (child.State.Status == StatusOfGame.INPROGRESS)
        {
          return child;
        }
      }

      for (int i = 0; i < node.Children.Count; ++i)
      {
        child = node.Children[i];
        if (child.State.Status == StatusOfGame.INPROGRESS)
        {
          return child;
        }
      }

      return child;
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
      if (node.State.Status != StatusOfGame.INPROGRESS)
      {
        return (int)node.State.Status;
      }
      else
      {
        return node.State.Simulate();
      }
    }

    private Moves GetTheBestMove(Node root)
    {
      Node node = null;
      int theBest = -1;

      foreach (var child in root.Children)
      {
        if (theBest < child.Visited)
        {
          theBest = child.Visited;
          node = child;
        }
      }

      return node.State.Moves;
    }
  }
}