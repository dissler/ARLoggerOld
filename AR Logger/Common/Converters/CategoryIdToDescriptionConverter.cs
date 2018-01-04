namespace AR_Logger.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// The category id to description converter.
    /// </summary>
    public class CategoryIdToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int && Values.CategoryDescriptions.ContainsKey((int)value)
                ? Values.CategoryDescriptions[(int)value]
                : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
