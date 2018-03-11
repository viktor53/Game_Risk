using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GameCore.Moves;
using Risk.Model.GamePlan;
using Risk.Networking.Messages.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Networking
{
  internal class Deserializer
  {
    private JsonSerializer _serializer;

    public Deserializer()
    {
      _serializer = new JsonSerializer();
    }

    public Area DeserializeArea(JToken data)
    {
      Area a = new Area((int)GetData<long>(data["ID"]), (int)GetData<long>(data["RegionID"]));
      a.ArmyColor = (ArmyColor)GetData<long>(data["ArmyColor"]);
      a.SizeOfArmy = (int)GetData<long>(data["SizeOfArmy"]);
      return a;
    }

    public GameBoardInfo DeserializeGameBoardInfo(JToken data)
    {
      var con = GetData<IList<IList<bool>>>(data["Connections"]);
      var areas = GetData<IList<AreaInfo>>(data["AreaInfos"]);
      return new GameBoardInfo(con, areas);
    }

    public Attack DeserializeAttackMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var attackerAreaID = (int)GetData<long>(data["AttackerAreaID"]);
      var defenderAreaID = (int)GetData<long>(data["DefenderAreaID"]);
      var attackSize = (AttackSize)GetData<long>(data["AttackSize"]);
      return new Attack(playerColor, attackerAreaID, defenderAreaID, attackSize);
    }

    public SetUp DeserializeSetUpMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var areaID = (int)GetData<long>(data["AreaID"]);
      return new SetUp(playerColor, areaID);
    }

    public Draft DeserializeDraftMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var areaID = (int)GetData<long>(data["AreaID"]);
      var numberOfUnit = (int)GetData<long>(data["NumberOfUnit"]);
      return new Draft(playerColor, areaID, numberOfUnit);
    }

    public Capture DeserilizeCaptureMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var armyToMove = (int)GetData<long>(data["ArmyToMove"]);
      return new Capture(playerColor, armyToMove);
    }

    public ExchangeCard DeserializeExchangeCardMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var combination = GetData<IList<RiskCard>>(data["Combination"]);
      return new ExchangeCard(playerColor, combination);
    }

    public Fortify DeserializeFortifyMove(JToken data)
    {
      var playerColor = (ArmyColor)GetData<long>(data["PlayerColor"]);
      var fromAreaID = (int)GetData<long>(data["FromAreaID"]);
      var toAreaID = (int)GetData<long>(data["ToAreaID"]);
      var sizeOfArmy = (int)GetData<long>(data["SizeOfArmy"]);
      return new Fortify(playerColor, fromAreaID, toAreaID, sizeOfArmy);
    }

    public T GetData<T>(JToken data)
    {
      using (JTokenReader reader = new JTokenReader(data))
      {
        return _serializer.Deserialize<T>(reader);
      }
    }
  }
}