namespace Risk.Model.GamePlan
{
  /// <summary>
  /// Represents only game plan. (areas and connections)
  /// </summary>
  public sealed class GamePlanInfo
  {
    /// <summary>
    /// Connections between areas.
    /// </summary>
    public bool[][] Connections { get; private set; }

    /// <summary>
    /// Areas on game plan.
    /// </summary>
    public Area[] Areas { get; private set; }

    /// <summary>
    /// Creates game plan information.
    /// </summary>
    /// <param name="connections">areas connections</param>
    /// <param name="areas">areas on game plan</param>
    public GamePlanInfo(bool[][] connections, Area[] areas)
    {
      Connections = connections;
      Areas = areas;
    }
  }
}