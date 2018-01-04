namespace AR_Logger.Dialogs
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for DateRangeDialog.xaml
    /// </summary>
    public partial class DateRangeDialog : Window
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRangeDialog"/> class.
        /// </summary>
        public DateRangeDialog()
        {
            this.InitializeComponent();

            // Initialize date pickers
            this.FirstDatePicker.PropertyChanged += (s, e) =>
            {
                this.FirstDate = this.FirstDatePicker.SelectedDate;
                this.LastDatePicker.FirstDate = this.FirstDatePicker.SelectedDate ?? DateTime.MinValue;
            };
            this.LastDatePicker.PropertyChanged += (s, e) =>
            {
                this.LastDate = this.LastDatePicker.SelectedDate;
                this.FirstDatePicker.LastDate = this.LastDatePicker.SelectedDate ?? DateTime.MaxValue;
            };
            this.FirstDatePicker.SelectedDate = DateTime.Today;
            this.FirstDatePicker.LastDate = DateTime.Today;
            this.LastDatePicker.SelectedDate = DateTime.Today;
            this.LastDatePicker.LastDate = DateTime.Today;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the first date.
        /// </summary>
        public DateTime? FirstDate { get; set; }

        /// <summary>
        /// Gets or sets the last date.
        /// </summary>
        public DateTime? LastDate { get; set; }

        #endregion // Properties

        #region Methods

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
