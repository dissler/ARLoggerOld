namespace AR_Logger.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// Returns a value indicating whether an array of booleans are all true.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class BooleanToEnabledMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToList().OfType<bool>().All(p => p);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
