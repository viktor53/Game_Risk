using Risk.Model.Enums;

namespace Risk.Model.GameCore.Moves
{
  /// <summary>
  /// Represents game move attack.
  /// </summary>
  public sealed class Attack : Move
  {
    /// <summary>
    /// Area of attacker, where attack comes from.
    /// </summary>
    public int AttackerAreaID { get; private set; }

    /// <summary>
    /// Area of defender, where attack goes.
    /// </summary>
    public int DefenderAreaID { get; private set; }

    /// <summary>
    /// Size of attack. One, two or three units.
    /// </summary>
    public AttackSize AttackSize { get; private set; }

    /// <summary>
    /// Creates game move attack.
    /// </summary>
    /// <param name="armyColor">color of player who attack</param>
    /// <param name="attackerAreaID">area of attacker</param>
    /// <param name="defenderAreaID">area of defender</param>
    /// <param name="attackSize">size of attack</param>
    public Attack(ArmyColor armyColor, int attackerAreaID, int defenderAreaID, AttackSize attackSize) : base(armyColor)
    {
      AttackerAreaID = attackerAreaID;
      DefenderAreaID = defenderAreaID;
      AttackSize = attackSize;
    }
  }
}