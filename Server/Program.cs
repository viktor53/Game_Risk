using Risk.Networking.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      RiskServer server = new RiskServer("Enterprise", 11000);

      server.Start();
    }
  }
}