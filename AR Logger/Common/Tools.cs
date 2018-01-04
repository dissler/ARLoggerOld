namespace AR_Logger.Common
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    using AR_Logger.Properties;

    /// <summary>
    /// Common functions and methods.
    /// </summary>
    internal class Tools
    {
        #region Functions

        #region Date formats

        /// <summary>
        /// Gets a list of dates for the given week of the given year.
        /// </summary>
        /// <param name="week">The week.</param>
        /// <param name="year">The year.</param>
        /// <returns>A <see cref="List{T}"/>.</returns>
        public static List<DateTime> GetDatesByWeekNum(int? week, int? year)
        {
            var returnDates = new List<DateTime>();
            if (week != null && year != null)
            {
                var checkDate = Convert.ToDateTime($"1-1-{year}");
                while (returnDates.Count < 7)
                {
                    if (GetWeekNumber(checkDate) == week)
                    {
                        returnDates.Add(checkDate);
                    }

                    checkDate = checkDate.AddDays(1);
                }
            }

            return returnDates;
        }

        /// <summary>
        /// Returns a formatted date and time stamp.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> formatted as "MM-dd-yy hh:mm:ss".</returns>
        public static string GetTimeStamp()
        {
            return $"{DateTime.Now:MM-dd-yy hh:mm:ss}";
        }

        /// <summary>
        /// Returns the weekday name for the specified date.
        /// </summary>
        /// <param name="thisDate">The date.</param>
        /// <returns>The localized day name.</returns>
        public static string GetWeekDayName(DateTime? thisDate)
        {
            return thisDate != null 
                ? Values.AppCulture.DateTimeFormat.GetDayName(((DateTime)thisDate).DayOfWeek)
                : string.Empty;
        }

        /// <summary>
        /// Returns the week number of the specified date, based on the full 7-day fiscal week.
        /// </summary>
        /// <param name="thisDate">The date.</param>
        /// <returns>The week number.</returns>
        public static int GetWeekNumber(DateTime? thisDate)
        {
            return thisDate != null 
                ? Values.Calendar.GetWeekOfYear((DateTime)thisDate, CalendarWeekRule.FirstFourDayWeek, GetWeekStart())
                : 0;
        }

        /// <summary>
        /// Returns the first day of the fiscal week from Settings (default Sunday).
        /// </summary>
        /// <returns>The first day of the fiscal week.</returns>
        public static DayOfWeek GetWeekStart()
        {
            var weekDay = Settings.Default.WeekStart;
            if (Enum.IsDefined(typeof(DayOfWeek), weekDay))
            {
                return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekDay, true);
            }

            return DayOfWeek.Sunday;
        }

        #endregion // Date formats

        /// <summary>
        /// Returns formatted username of current user (assumes firstname.lastname format).
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetUserInitials()
        {
            var user = Environment.UserName;
            return $"{user.Substring(0, 1).ToUpper()}{user.Substring(user.IndexOf('.') + 1, 1).ToUpper()}";
        }

        #endregion // Functions

        #region Methods

        /// <summary>
        /// Writes a time-stamped message to console output.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sender">The sender.</param>
        public static void DebugLog(string message, [CallerMemberName] string sender = null)
        {
            var output = $"[{GetTimeStamp()} {sender}] {message}";
            Console.WriteLine(output);
        }

        /// <summary>
        /// The unhandled exception handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public static void UnhandledExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            // Write error to log
            DebugLog($"Unhandled exception from '{sender}': {e.Exception.Message}");
        }

        #region Data Display

        /// <summary>
        /// Displays the contents of a data set in a window
        /// with a drop-down to select each data table.
        /// </summary>
        /// <param name="source">The <see cref="DataSet"/>.</param>
        public static void DisplayDataSet(DataSet source)
        {
            var tableNames = new ComboBox
            {
                DisplayMemberPath = "TableName",
                ItemsSource = source.Tables,
                Margin = new Thickness(3)
            };
            var tableGrid = BuildDataDisplayGrid();
            tableNames.SelectionChanged += (s, e) =>
            {
                tableGrid.ItemsSource = source.Tables[tableNames.SelectedIndex].DefaultView;
            };
            tableNames.SelectedIndex = 0;
            var content = new Grid
            {
                RowDefinitions = { new RowDefinition { Height = GridLength.Auto }, new RowDefinition() }
            };
            content.Children.Add(tableNames);
            content.Children.Add(tableGrid);
            Grid.SetRow(tableGrid, 1);
            ShowDataDisplayWindow(content, source.DataSetName);
        }

        /// <summary>
        /// Displays the contents of a data table in a window.
        /// </summary>
        /// <param name="table">The <see cref="DataTable    "/>.</param>
        public static void DisplayDataTable(DataTable table)
        {
            var grid = BuildDataDisplayGrid();
            grid.ItemsSource = table.DefaultView;
            ShowDataDisplayWindow(grid, table.TableName);
        }

        private static DataGrid BuildDataDisplayGrid(int margin = 3)
        {
            return new DataGrid
            {
                AutoGenerateColumns = true,
                IsReadOnly = true,
                Margin = new Thickness(margin)
            };
        }

        private static void ShowDataDisplayWindow(object content, string title)
        {
            var displayWindow = new Window
            {
                Content = content,
                Height = 400,
                MinHeight = 400,
                MinWidth = 600,
                Title = title,
                Topmost = true,
                Visibility = Visibility.Visible,
                Width = 600
            };
        }

        #endregion // Data Display

        public static void RunTaskAsync<T>(
            Func<Task<T>> task, 
            Action<object> continueWith, 
            CancellationToken canceller, 
            IProgress<int> progress = null)
        {
            var syncContext = SynchronizationContext.Current != null
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Default;
            Task.Run(() => task, canceller)
                .ContinueWith(
                t => { continueWith(t.Result); },
                canceller,
                TaskContinuationOptions.None,
                syncContext);
        }

        #endregion // Methods
    }
}
