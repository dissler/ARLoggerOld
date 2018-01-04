namespace AR_Logger.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// Returns a value indicating whether an array of objects are all the same.
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    internal class AreObjectsSameMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(p => p.Equals(values[0]));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
