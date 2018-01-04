namespace AR_Logger.Dialogs
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;

    using AR_Logger.Data;

    /// <summary>
    /// Interaction logic for ImportDialog.xaml
    /// </summary>
    public partial class ImportDialog : Window
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDialog"/> class.
        /// </summary>
        /// <param name="importTable">The data table to import.</param>
        public ImportDialog(LogTable importTable)
        {
            this.InitializeComponent();

            // Initialize grid view
            this.ImportTable = importTable;
            this.ImportGrid.DisplayView = this.ImportTable.DefaultView;

            // Initialize date picker
            this.DatePicker.FirstDate = this.ImportTable.FirstDate;
            this.DatePicker.LastDate = this.ImportTable.LastDate;
            this.DatePicker.PropertyChanged += this.OnDatePickerPropertyChanged;
            this.DatePicker.SelectedDate = this.DatePicker.FirstDate;

            // Watch for selected rows
            this.ImportTable.RowChanged += (s, e) => { this.SetButtonEnabledState(); };
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the import table.
        /// </summary>
        public LogTable ImportTable { get; set; }

        /// <summary>
        /// Whether there are rows selected for import.
        /// </summary>
        public bool RowsSelected => this.ImportTable.Rows.OfType<LogRow>().Any(row => row.Import);

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Sets button enabled state depending on how many rows are selected for import.
        /// </summary>
        private void SetButtonEnabledState()
        {
            this.ImportButton.IsEnabled = this.ImportTable.Rows.OfType<LogRow>().Any(row => row.Import);
            this.SelectAllButton.IsEnabled = this.ImportTable.Rows.OfType<LogRow>().Any(row => !row.Import);
            this.SelectNoneButton.IsEnabled = this.ImportTable.Rows.OfType<LogRow>().Any(row => row.Import);
        }

        /// <summary>
        /// Handles changes in the date picker.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDatePickerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.DatePicker.SelectedDate))
            {
                this.ImportGrid.DisplayView.RowFilter = $"([{LogTable.DateColumnName}] = '{this.DatePicker.SelectedDate:d}')";
            }
        }

        /// <summary>
        /// Selects all rows for import.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSelectAllButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var row in this.ImportTable.Rows.OfType<LogRow>())
            {
                row.Import = true;
            }
        }

        /// <summary>
        /// Deselects all rows.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSelectNoneButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var row in this.ImportTable.Rows.OfType<LogRow>())
            {
                row.Import = false;
            }
        }

        /// <summary>
        /// Submits the dialog.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Submit(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        #endregion // Methods
    }
}
