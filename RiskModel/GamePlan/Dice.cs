using System;

namespace Risk.Model.GamePlan
{
  /// <summary>
  /// Represents dice and provides roll of the dice.
  /// </summary>
  public sealed class Dice
  {
    private Random _ran;

    /// <summary>
    /// Creates default dice.
    /// </summary>
    public Dice()
    {
      _ran = new Random();
    }

    /// <summary>
    /// Rolls the dice.
    /// </summary>
    /// <param name="countOfDice">number of dice</param>
    /// <returns>1-3 rolls of the dice otherwise null</returns>
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

    /// <summary>
    /// Rolls three dice.
    /// </summary>
    /// <returns>three rolls</returns>
    private int[] RollThreeDice()
    {
      int[] rolls = new int[3];

      for (int i = 0; i < rolls.Length; i++)
      {
        rolls[i] = _ran.Next(1, 7);
      }

      Sort(rolls);

      return rolls;
    }

    /// <summary>
    /// Rolls two dice.
    /// </summary>
    /// <returns>two rolls</returns>
    private int[] RollTwoDice()
    {
      int[] rolls = new int[2];

      for (int i = 0; i < rolls.Length; i++)
      {
        rolls[i] = _ran.Next(1, 7);
      }

      Sort(rolls);

      return rolls;
    }

    /// <summary>
    /// Rolls one die.
    /// </summary>
    /// <returns></returns>
    private int[] RollOneDie()
    {
      return new int[] { _ran.Next(1, 7) };
    }

    /// <summary>
    /// Sorts rolls.
    /// </summary>
    /// <param name="rolls">rolls the dice</param>
    private void Sort(int[] rolls)
    {
      for (int i = 1; i < rolls.Length; ++i)
      {
        for (int j = 0; j < rolls.Length - i; ++j)
        {
          if (rolls[j] < rolls[j + 1])
          {
            int temp = rolls[j];
            rolls[j] = rolls[j + 1];
            rolls[j + 1] = temp;
          }
        }
      }
    }
  }
}