using Risk.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Risk.ViewModel.Converters
{
  public sealed class EnumToColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      SolidColorBrush color = new SolidColorBrush();
      ArmyColor ac = (ArmyColor)Enum.ToObject(typeof(ArmyColor), value);
      switch(ac)
      {
        case ArmyColor.Neutral:
          color.Color = Colors.Gray;
          break;
        case ArmyColor.Green:
          color.Color = Colors.Green;
          break;
        case ArmyColor.Red:
          color.Color = Colors.Red;
          break;
        case ArmyColor.White:
          color.Color = Colors.White;
          break;
        case ArmyColor.Yellow:
          color.Color = Colors.Yellow;
          break;
        case ArmyColor.Blue:
          color.Color = Colors.Blue;
          break;
        case ArmyColor.Black:
          color.Color = Colors.Black;
          break;
      }
      return color;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
