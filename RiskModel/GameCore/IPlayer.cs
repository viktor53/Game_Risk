using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Risk.Model.Cards;
using Risk.Model.Enums;

namespace Risk.Model.GameCore
{
  public interface IPlayer
  {
    ArmyColor PlayerColor { get; }

    int FreeUnit { get; set; }

    IList<RiskCard> Cards { get; }

    void PlaySetUp();

    void PlayDraft();

    void PlayAttack();

    void PlayFortify();
  }
}