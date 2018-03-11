using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Cards;
using Risk.Model.Enums;
using Risk.Model.GamePlan;

namespace Risk.Model.GameCore
{
  public interface IPlayer
  {
    ArmyColor PlayerColor { get; set; }

    int FreeUnit { get; set; }

    void AddCard(RiskCard card);

    void RemoveCard(RiskCard card);

    void StartPlayer(IGame game);

    void PlaySetUp();

    void PlayDraft();

    void PlayAttack();

    void PlayFortify();

    Task UpdateGame(Area area);

    void EndPlayer(bool isWinner);
  }
}