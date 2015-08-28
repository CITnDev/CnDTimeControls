using System;
using System.Globalization;
using System.Windows.Data;

namespace CnDTimeLineSample
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime) value).ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {            
            if (value != null && value.Length == 2)
            {
                DateTime dt = default(DateTime);
                TimeZone tz = default(TimeZone);

                if (value[0] is DateTime)
                    dt = (DateTime) value[0];
                if (value[1] is DateTime)
                    dt = (DateTime) value[1];

                if (value[0] is TimeZone)
                    tz = (TimeZone) value[0];
                if (value[1] is TimeZone)
                    tz = (TimeZone) value[1];

                if (tz == default(TimeZone))
                    return "TimeZone not set.";

                if (dt == default(DateTime))
                    return "DateTime not set";

                return System.Convert.ChangeType(tz.ToLocalTime(dt), targetType, culture);
            }

            return System.Convert.ChangeType(DateTime.Now, targetType, culture);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();

            //if (targetType != null && targetType.Length == 2)
            //{
            //    DateTime dt = default(DateTime);
            //    TimeZone tz = default(TimeZone);

            //    if (value[0] is DateTime)
            //        dt = (DateTime) value[0];
            //    if (value[1] is DateTime)
            //        dt = (DateTime) value[1];

            //    if (value[0] is TimeZone)
            //        tz = (TimeZone) value[0];
            //    if (value[1] is TimeZone)
            //        tz = (TimeZone) value[1];

            //    if (tz == default(TimeZone) || dt == default(DateTime))
            //        return DateTime.Now;

            //    return tz.ToLocalTime(dt);
            //}

            //return DateTime.Now;
        }
    }


    public class TimeUtcConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = (DateTime)value;
            var utc = d.ToUniversalTime();

            return System.Convert.ChangeType(utc, targetType, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();

            //if (targetType != null && targetType.Length == 2)
            //{
            //    DateTime dt = default(DateTime);
            //    TimeZone tz = default(TimeZone);

            //    if (value[0] is DateTime)
            //        dt = (DateTime) value[0];
            //    if (value[1] is DateTime)
            //        dt = (DateTime) value[1];

            //    if (value[0] is TimeZone)
            //        tz = (TimeZone) value[0];
            //    if (value[1] is TimeZone)
            //        tz = (TimeZone) value[1];

            //    if (tz == default(TimeZone) || dt == default(DateTime))
            //        return DateTime.Now;

            //    return tz.ToLocalTime(dt);
            //}

            //return DateTime.Now;
        }
    }
}
