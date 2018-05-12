using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal class Node
  {
    private Node _parent;

    private List<Node> _children;

    private State _state;

    private int _visited;

    private int _wins;

    public Node Parent
    {
      get
      {
        return _parent;
      }
      set
      {
        _parent = value;
      }
    }

    public List<Node> Children
    {
      get
      {
        return _children;
      }
      set
      {
        _children = value;
      }
    }

    public State State
    {
      get
      {
        return _state;
      }
      set
      {
        _state = value;
      }
    }

    public int Visited
    {
      get
      {
        return _visited;
      }
      set
      {
        _visited = value;
      }
    }

    public int Wins
    {
      get
      {
        return _wins;
      }
      set
      {
        _wins = value;
      }
    }

    public Node(Node parent, State state)
    {
      _parent = parent;
      _state = state;
      _children = new List<Node>();
      _visited = 0;
      _wins = 0;
    }
  }
}