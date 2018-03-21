using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Risk.Model.Enums;

namespace Risk.ViewModel.Converters
{
  /// <summary>
  /// Represents converter from enum ArmyColor to color SolidColorBrush.
  /// </summary>
  public sealed class EnumToColorConverter : IValueConverter
  {
    /// <summary>
    /// Converts value ArmyColor to color SolidColorBrush.
    /// </summary>
    /// <param name="value">ArmyColor</param>
    /// <param name="targetType">targetType is not used</param>
    /// <param name="parameter">parametr is not used</param>
    /// <param name="culture">culture is not used</param>
    /// <returns>converted ArmyColor</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      SolidColorBrush color = new SolidColorBrush();
      ArmyColor ac = (ArmyColor)Enum.ToObject(typeof(ArmyColor), value);
      switch (ac)
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

    /// <summary>
    /// Method is not implemented.
    /// </summary>
    /// <param name="value">value is not used</param>
    /// <param name="targetType">targetType is not used</param>
    /// <param name="parameter">parametr is not used</param>
    /// <param name="culture">culture is not used</param>
    /// <returns>NotImplementedException</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}