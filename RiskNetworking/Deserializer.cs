using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Networking.Messages.Data;

namespace Risk.Networking
{
  /// <summary>
  /// Represents JSON deserializer of bigger data in messages between client and server.
  /// </summary>
  internal class Deserializer
  {
    private JsonSerializer _serializer;

    /// <summary>
    /// Initialize deserializer using JsonSerializer.
    /// </summary>
    public Deserializer()
    {
      _serializer = new JsonSerializer();
    }

    /// <summary>
    /// Deserializes area JSON data.
    /// </summary>
    /// <param name="data">data containing area as JSON</param>
    /// <returns>deserialized area</returns>
    public Area DeserializeArea(JToken data)
    {
      Area a = new Area((byte)GetData<long>(data["ID"]), (byte)GetData<long>(data["RegionID"]));
      a.ArmyColor = (ArmyColor)GetData<long>(data["ArmyColor"]);
      a.SizeOfArmy = (int)GetData<long>(data["SizeOfArmy"]);
      return a;
    }

    /// <summary>
    /// Deserializes game board information JSON data.
    /// </summary>
    /// <param name="data">data containing game board inforamtion</param>
    /// <returns>deseriliazed game board information</returns>
    public GameBoardInfo DeserializeGameBoardInfo(JToken data)
    {
      var con = GetData<IList<IList<bool>>>(data["Connections"]);
      var areas = GetData<IList<AreaInfo>>(data["AreaInfos"]);
      return new GameBoardInfo(con, areas);
    }

    /// <summary>
    /// Deserializes attack move JSON data.
    /// </summary>
    /// <param name="data">data containing attack move</param>
    /// <returns>deserialized attack move</returns>
    public Attack DeserializeAttackMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var attackerAreaID = (int)GetData<long>(data["AttackerAreaID"]);
      var defenderAreaID = (int)GetData<long>(data["DefenderAreaID"]);
      var attackSize = (AttackSize)GetData<long>(data["AttackSize"]);
      return new Attack(playerColor, attackerAreaID, defenderAreaID, attackSize);
    }

    /// <summary>
    /// Deserializes setup move JSON data.
    /// </summary>
    /// <param name="data">data containing setup move</param>
    /// <returns>deserialized setup move</returns>
    public SetUp DeserializeSetUpMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var areaID = (int)GetData<long>(data["AreaID"]);
      return new SetUp(playerColor, areaID);
    }

    /// <summary>
    /// Deserializes draft move JSON data.
    /// </summary>
    /// <param name="data">data containing draft move</param>
    /// <returns>deserialized draft move</returns>
    public Draft DeserializeDraftMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var areaID = (int)GetData<long>(data["AreaID"]);
      var numberOfUnit = (int)GetData<long>(data["NumberOfUnit"]);
      return new Draft(playerColor, areaID, numberOfUnit);
    }

    /// <summary>
    /// Deserializes capture move JSON data.
    /// </summary>
    /// <param name="data">data containing capture move</param>
    /// <returns>deserialized capture move</returns>
    public Capture DeserilizeCaptureMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var armyToMove = (int)GetData<long>(data["ArmyToMove"]);
      return new Capture(playerColor, armyToMove);
    }

    /// <summary>
    /// Deserializes fortify move JSON data.
    /// </summary>
    /// <param name="data">data containing fortify move</param>
    /// <returns>deserialized fortify move</returns>
    public Fortify DeserializeFortifyMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var fromAreaID = (int)GetData<long>(data["FromAreaID"]);
      var toAreaID = (int)GetData<long>(data["ToAreaID"]);
      var sizeOfArmy = (int)GetData<long>(data["SizeOfArmy"]);
      return new Fortify(playerColor, fromAreaID, toAreaID, sizeOfArmy);
    }

    /// <summary>
    /// Deserializes data of type T.
    /// </summary>
    /// <typeparam name="T">type of data that will be deserialized</typeparam>
    /// <param name="data">data containing object of type T</param>
    /// <returns>deserialized data of type T</returns>
    public T GetData<T>(JToken data)
    {
      using (JTokenReader reader = new JTokenReader(data))
      {
        return _serializer.Deserialize<T>(reader);
      }
    }
  }
}