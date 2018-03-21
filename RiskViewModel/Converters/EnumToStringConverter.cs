using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Risk.ViewModel.Converters
{
  /// <summary>
  /// Represents converter from enum to string.
  /// </summary>
  public sealed class EnumToStringConverter : IValueConverter
  {
    /// <summary>
    /// Converts enum to string.
    /// </summary>
    /// <param name="value">enum to converting</param>
    /// <param name="targetType">targetType is not used</param>
    /// <param name="parameter">parametr is not used</param>
    /// <param name="culture">culture is not used</param>
    /// <returns>converted enum or empty string</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string EnumString;
      try
      {
        EnumString = Enum.GetName((value.GetType()), value);
        return EnumString;
      }
      catch
      {
        return string.Empty;
      }
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