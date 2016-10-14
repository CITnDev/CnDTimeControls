using System;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CnDTimeControls.Converters
{
    public class TimeBandItemLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = (double)value/2;
            return new Thickness(-d,0,0,0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
