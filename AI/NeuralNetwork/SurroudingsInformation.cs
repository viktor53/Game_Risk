using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.AI.NeuralNetwork
{
  /// <summary>
  /// Represents inforamtion about surroundings of an area.
  /// </summary>
  internal class SurroundingsInformation
  {
    public int[] NumberOfFriends { get; private set; }

    public int[] NumberOfEnemies { get; private set; }

    public int[] FriendlyArmies { get; private set; }

    public int[] EnemyArmies { get; private set; }

    public SurroundingsInformation(int[] numberOfFriends, int[] numberOfEnemies, int[] friendlyArmies, int[] enemyArmies)
    {
      NumberOfFriends = numberOfFriends;
      NumberOfEnemies = numberOfFriends;
      FriendlyArmies = friendlyArmies;
      EnemyArmies = enemyArmies;
    }
  }
}