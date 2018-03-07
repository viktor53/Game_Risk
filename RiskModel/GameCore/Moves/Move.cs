using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Risk.Model.GameCore.Moves
{
  public abstract class Move
  {
    public ArmyColor PlayerColor { get; private set; }

    public Move(ArmyColor playerColor)
    {
      PlayerColor = playerColor;
    }
  }
}
