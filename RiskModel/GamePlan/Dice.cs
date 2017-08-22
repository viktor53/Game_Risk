using System;

namespace Risk.Model.GamePlan
{
  sealed class Dice
  {
    private Random _ran;

    public Dice()
    {
      _ran = new Random();
    }

    public int[] RollDice(int countOfDice)
    {
      switch (countOfDice)
      {
        case 1:
          return RollOneDie();
        case 2:
          return RollTwoDice();
        case 3:
          return RollThreeDice();
        default:
          return null;
      }
    }

    private int[] RollThreeDice()
    {
      int[] rolls = new int[3];

      for (int i = 0; i < rolls.Length; i++)
      {
        rolls[i] = _ran.Next(0, 6);
      }

      return rolls;
    }

    private int[] RollTwoDice()
    {
      int[] rolls = new int[2];

      for (int i = 0; i < rolls.Length; i++)
      {
        rolls[i] = _ran.Next(0, 6);
      }

      return rolls;
    }

    private int[] RollOneDie()
    {
      return new int[] { _ran.Next(0, 6) };
    }
  }
}
