namespace AR_Logger.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using AR_Logger.Data;

    using Calendar = System.Globalization.Calendar;

    /// <summary>
    /// Stores common values.
    /// </summary>
    internal class Values
    {
        #region File Paths

        /// <summary>
        /// Gets the log file name.
        /// </summary>
        public static string FilenameLog { get; } = "App.log";

        /// <summary>
        /// Gets the WSR file extension.
        /// </summary>
        public static string WsrExtension { get; } = ".xls";

        #endregion // File Paths

        #region Sql

        /// <summary>
        /// Gets the minimum interval before retrying a remote connection.
        /// </summary>
        public static TimeSpan MinSyncInterval { get; } = TimeSpan.FromMinutes(1);

        #endregion // Sql

        #region Regional Formats

        /// <summary>
        /// Gets the current app culture.
        /// </summary>
        public static CultureInfo AppCulture { get; } = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// Gets the calendar.
        /// </summary>
        public static Calendar Calendar { get; } = AppCulture.Calendar;

        #endregion // Regional Formats

        #region Grid Formats

        /// <summary>
        /// Gets the list of column names to hide when displaying account history.
        /// </summary>
        public static List<string> AccountHiddenColumnNames { get; } = new List<string>
        {
            LogTable.ImportColumnName,
            LogTable.AccountIdColumnName,
            LogTable.AccountNameColumnName,
            LogTable.AccountNumColumnName,
            LogTable.CityColumnName,
            LogTable.RecordIdColumnName,
            LogTable.StateColumnName,
            LogTable.StreetColumnName,
            LogTable.ZoneColumnName
        };

        /// <summary>
        /// Gets the list of column names to hide when displaying grid for import.
        /// </summary>
        public static List<string> ImportHiddenColumnNames { get; } = new List<string>
        {
            LogTable.AccountIdColumnName,
            LogTable.AccountNameColumnName,
            LogTable.DateColumnName,
            LogTable.RecordIdColumnName,
            LogTable.StreetColumnName
        };

        /// <summary>
        /// Gets the list of column names to hide when displaying the log.
        /// </summary>
        public static List<string> LogHiddenColumnNames { get; } = new List<string>
        {
            LogTable.AccountIdColumnName,
            LogTable.AccountNameColumnName,
            LogTable.DateColumnName,
            LogTable.ImportColumnName,
            LogTable.RecordIdColumnName,
            LogTable.StreetColumnName
        };

        /// <summary>
        /// Gets the list of read only column names when displaying the log.
        /// </summary>
        public static List<string> LogReadOnlyColumnNames { get; } = new List<string>
        {
            LogTable.AccountNumColumnName,
            LogTable.CategoryIdColumnName,
            LogTable.CityColumnName,
            LogTable.StateColumnName
        };

        /// <summary>
        /// Gets the log column name to set to width="*".
        /// </summary>
        public static string LogStretchColumnName { get; } = LogTable.NotesColumnName;

        /// <summary>
        /// Gets the list of column names to hide when displaying tickets.
        /// </summary>
        public static List<string> TicketHiddenColumnNames { get; } = new List<string>
        {
            TicketTable.AccountIdColumnName,
            TicketTable.TicketIdColumnName
        };

        /// <summary>
        /// Gets the ticket column name to set to width="*".
        /// </summary>
        public static string TicketStretchColumnName { get; } = TicketTable.NotesColumnName;

        #endregion // Grid Formats

        #region Category Formats

        /// <summary>
        /// Gets the dictionary of category descriptions.
        /// </summary>
        public static Dictionary<int, string> CategoryDescriptions { get; } = new Dictionary<int, string>
        {
            { 0, "No category" }
        };

        /// <summary>
        /// Gets the dictionary of category menu icons. Category IDs will be added at runtime.
        /// </summary>
        public static Dictionary<string, BitmapImage> CategoryMenuIcons { get; } = new Dictionary<string, BitmapImage>
        {
            { "Ignore", (BitmapImage)Application.Current.Resources["CategoryImageIgnore"] },
            { "Network", (BitmapImage)Application.Current.Resources["CategoryImageNetwork"] },
            { "OT", (BitmapImage)Application.Current.Resources["CategoryImageOt"] },
            { "Other Software", (BitmapImage)Application.Current.Resources["CategoryImageOther"] },
            { "POS", (BitmapImage)Application.Current.Resources["CategoryImagePos"] },
            { "Procedural", (BitmapImage)Application.Current.Resources["CategoryImageProcedural"] },
            { "Unknown", (BitmapImage)Application.Current.Resources["CategoryImageUnknown"] }
        };

        #endregion // Category Formats

        #region State Values

        /// <summary>
        /// Gets the string to write in place of boolean true.
        /// </summary>
        public static string BoolExportString { get; } = "X";

        /// <summary>
        /// Gets the empty view filter string so we can distinguish between
        /// an unset view filter and a view filter set to view blank items.
        /// </summary>
        public static string EmptyViewFilterString { get; } = "<none>";

        #endregion // State Values
    }
}
