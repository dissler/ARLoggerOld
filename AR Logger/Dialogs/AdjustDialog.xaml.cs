namespace AR_Logger.Dialogs
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using AR_Logger.Common;
    using AR_Logger.Data;

    /// <summary>
    /// Interaction logic for AdjustDialog.xaml
    /// </summary>
    public partial class AdjustDialog : Window
    {
        #region Fields

        /// <summary>
        /// The selected log row property.
        /// </summary>
        private static readonly DependencyProperty SelectedLogRowProperty;

        private bool isSettingUiState;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="AdjustDialog"/> class.
        /// </summary>
        static AdjustDialog()
        {
            SelectedLogRowProperty = DependencyProperty.Register(
                nameof(AdjustDialog.SelectedLogRow),
                typeof(LogRow),
                typeof(AdjustDialog),
                new PropertyMetadata(null, OnSelectedLogRowChanged));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustDialog"/> class.
        /// </summary>
        public AdjustDialog()
        {
            this.InitializeComponent();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the selected log row.
        /// </summary>
        public LogRow SelectedLogRow
        {
            get { return (LogRow)this.GetValue(SelectedLogRowProperty); }
            set { this.SetValue(SelectedLogRowProperty, value); }
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Handles changes in the selected log row property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSelectedLogRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as AdjustDialog;
            if (me != null)
            {
                me.AccountDetailsControl.SelectedAccount = me.SelectedLogRow;
                me.DateLabel.Text = $"{me.SelectedLogRow.Date:d}";
                me.ImbalanceAmount.Text = $"{me.SelectedLogRow.Amount:n2}";
            }
        }

        /// <summary>
        /// Enforces numeric (0-9, ., -) text input.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void EnforceNumericInput(object sender, TextCompositionEventArgs e)
        {
            var numericInput = new Regex("[^0-9.-]+");
            e.Handled = numericInput.IsMatch(e.Text);
        }

        /// <summary>
        /// Processes text input.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ProcessTextInput(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !this.isSettingUiState)
            {
                this.isSettingUiState = true;

                if (textBox.Equals(this.ImbalanceAmount) || textBox.Equals(this.OldAmount))
                {
                    // Calculate amounts
                    decimal imbalanceAmount;
                    decimal oldAmount;
                    if (decimal.TryParse(this.ImbalanceAmount.Text, out imbalanceAmount)
                        && decimal.TryParse(this.OldAmount.Text, out oldAmount))
                    {
                        this.NewAmount.Text = $"{oldAmount - imbalanceAmount}";
                    }
                }
                else if (textBox.Equals(this.TransactionId))
                {
                    // Check transaction date
                    DateTime transDate;
                    var validInput = this.TransactionId.Text.Length >= 6
                        && DateTime.TryParseExact(
                            this.TransactionId.Text.Substring(0, 6),
                            "yyMMdd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out transDate)
                        && transDate.Date == this.SelectedLogRow.Date.Date;
                    this.ErrorLabel.Visibility = validInput
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    this.SubmitButton.IsEnabled = validInput;
                }

                this.isSettingUiState = false;
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
