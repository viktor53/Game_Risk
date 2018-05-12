using System.Collections.Generic;

namespace Risk.Networking.Messages.Data
{
  /// <summary>
  /// Message data containing game board information.
  /// </summary>
  public class GameBoardInfo
  {
    /// <summary>
    /// Connections of areas.
    /// </summary>
    public IList<IList<bool>> Connections { get; private set; }

    /// <summary>
    /// Areas informations.
    /// </summary>
    public IList<AreaInfo> AreaInfos { get; private set; }

    /// <summary>
    /// Creates information about game board.
    /// </summary>
    /// <param name="con">connections of areas</param>
    /// <param name="areaInfos">areas of game board informations</param>
    public GameBoardInfo(IList<IList<bool>> con, IList<AreaInfo> areaInfos)
    {
      Connections = con;
      AreaInfos = areaInfos;
    }
  }
}