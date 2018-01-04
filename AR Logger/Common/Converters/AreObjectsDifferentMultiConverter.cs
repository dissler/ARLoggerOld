namespace AR_Logger.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Returns a value indicating whether there is at least one object in 
    /// the array of values whose string representation is different than the others.
    /// Returns false if any values are unset dependency properties.
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    internal class AreObjectsDifferentMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(p => !p.Equals(DependencyProperty.UnsetValue)) && values.Any(p => p.ToString() != values[0].ToString());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
