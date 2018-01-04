namespace AR_Logger.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// The category id to image converter.
    /// </summary>
    public class CategoryIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && Values.CategoryMenuIcons.ContainsKey(value.ToString())
                ? Values.CategoryMenuIcons[value.ToString()]
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
