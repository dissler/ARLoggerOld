namespace AR_Logger.Common.Controls
{
    using System.Data;
    using System.Windows;
    using System.Windows.Controls;

    using AR_Logger.Data;

    /// <summary>
    /// Interaction logic for AccountDetailsControl.xaml
    /// </summary>
    public partial class AccountDetailsControl : UserControl
    {
        #region Fields

        /// <summary>
        /// The account table property.
        /// </summary>
        public static readonly DependencyProperty AccountsProperty;

        /// <summary>
        /// The is read only property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty;

        /// <summary>
        /// The selected account property.
        /// </summary>
        public static readonly DependencyProperty SelectedAccountProperty;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="AccountDetailsControl"/> class.
        /// </summary>
        static AccountDetailsControl()
        {
            AccountsProperty = DependencyProperty.Register(
                nameof(AccountDetailsControl.Accounts),
                typeof(DataView),
                typeof(AccountDetailsControl),
                new PropertyMetadata(null, OnAccountTableChanged));
            IsReadOnlyProperty = DependencyProperty.Register(
                nameof(AccountDetailsControl.IsReadOnly),
                typeof(bool),
                typeof(AccountDetailsControl),
                new PropertyMetadata(true, OnIsReadOnlyChanged));
            SelectedAccountProperty = DependencyProperty.Register(
                nameof(AccountDetailsControl.SelectedAccount),
                typeof(LogRow),
                typeof(AccountDetailsControl),
                new PropertyMetadata(null, OnSelectedAccountChanged));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDetailsControl"/> class.
        /// </summary>
        public AccountDetailsControl()
        {
            this.InitializeComponent();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the accounts view.
        /// </summary>
        public DataView Accounts
        {
            get { return (DataView)this.GetValue(AccountsProperty); }
            set { this.SetValue(AccountsProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the account details are read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected account.
        /// </summary>
        public LogRow SelectedAccount
        {
            get { return (LogRow)this.GetValue(SelectedAccountProperty); }
            set { this.SetValue(SelectedAccountProperty, value); }
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Handles changes in the account table property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnAccountTableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as AccountDetailsControl;
            if (me?.ComboBox != null)
            {
                me.ComboBox.ItemsSource = me.Accounts.Table.Rows;
            }
        }

        /// <summary>
        /// Handles changes in the is read only property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as AccountDetailsControl;
            if (me != null)
            {
                // Set visibility
                me.ComboBox.Visibility = me.IsReadOnly ? Visibility.Collapsed : Visibility.Visible;
                me.AccountNumLabel.Visibility = me.IsReadOnly ? Visibility.Visible : Visibility.Collapsed;

                // Set editability
                //me.AccountStreet.IsReadOnly = me.IsReadOnly;
                //me.AccountCity.IsReadOnly = me.IsReadOnly;
                //me.AccountState.IsReadOnly = me.IsReadOnly;
                //me.AccountZone.IsReadOnly = me.IsReadOnly;
            }
        }

        /// <summary>
        /// Handles changes in the selected account property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSelectedAccountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as AccountDetailsControl;
            if (me != null)
            {
                me.ComboBox.SelectedItem = me.SelectedAccount;
                me.AccountNumLabel.Text = $"{me.SelectedAccount?.AccountNum}";
                me.AccountStreet.Text = $"{me.SelectedAccount?.Street}";
                me.AccountCity.Text = $"{me.SelectedAccount?.City}";
                me.AccountState.Text = $"{me.SelectedAccount?.State}";
                me.AccountZone.Text = $"{me.SelectedAccount?.Zone}";
            }
        }

        /// <summary>
        /// Handles changes in the selected account.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedAccount = this.ComboBox.SelectedItem as LogRow;
        }

        #endregion // Methods
    }
}
