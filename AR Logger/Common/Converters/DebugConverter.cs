namespace AR_Logger.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// The debug converter.
    /// </summary>
    internal class DebugConverter : IValueConverter, IMultiValueConverter
    {
        #region IValue

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Tools.DebugLog($"Object: '{value}'.");

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion // IValue

        #region IMultiValue

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion // IMultiValue
    }
}
