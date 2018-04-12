using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.MCTS
{
  internal static class UCT
  {
    private static double CountUCT(int parentVisit, int visit, int wins)
    {
      const double c = 1.4142;

      if (visit == 0)
      {
        return double.MaxValue;
      }

      return ((double)wins / visit) + c * Math.Sqrt(Math.Log(parentVisit) / visit);
    }

    public static Node GetBestNodeUsingUCT(Node node)
    {
      int parentVisit = node.Visited;

      Node result = null;
      double maxUTC = double.MinValue;

      foreach (var n in node.Children)
      {
        double utc = CountUCT(parentVisit, n.Visited, n.Wins);
        if (utc > maxUTC)
        {
          maxUTC = utc;
          result = n;
        }
      }

      return result;
    }
  }
}