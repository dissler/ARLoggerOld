namespace AR_Logger.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AssignCasesDialog.xaml
    /// </summary>
    public partial class AssignCasesDialog : Window
    {
        #region Fields

        /// <summary>
        /// The available techs property.
        /// </summary>
        private static readonly DependencyProperty AvailableTechsProperty;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="AssignCasesDialog"/> class.
        /// </summary>
        static AssignCasesDialog()
        {
            AvailableTechsProperty = DependencyProperty.Register(
                nameof(AssignCasesDialog.AvailableTechs),
                typeof(List<string>),
                typeof(AssignCasesDialog),
                new PropertyMetadata(new List<string>(), OnAvailableTechsChanged));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignCasesDialog"/> class.
        /// </summary>
        /// <param name="techList">The list of available technicians.</param>
        /// <param name="selectedDate">The date to display.</param>
        public AssignCasesDialog(List<string> techList, DateTime? selectedDate)
        {
            this.InitializeComponent();

            // Initialize properties
            this.AvailableTechs = techList;
            this.DateText.Text = $"{selectedDate:d}";
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets the list of available techs.
        /// </summary>
        public List<string> AvailableTechs
        {
            get
            {
                return (List<string>)this.GetValue(AvailableTechsProperty);
            }

            private set
            {
                this.SetValue(AvailableTechsProperty, value);
            }
        }

        /// <summary>
        /// The list of selected techs.
        /// </summary>
        public List<string> SelectedTechs => this.TechList.SelectedItems.OfType<string>().ToList();

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Handles changes in the list of available techs.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnAvailableTechsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as AssignCasesDialog;
            if (me?.TechList != null)
            {
                me.AvailableTechs.Sort();
                me.TechList.ItemsSource = me.AvailableTechs;
            }
        }

        /// <summary>
        /// Adds a tech to the list.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void OnAddTechClicked(object sender, RoutedEventArgs e)
        {
            if (this.NewTech.Text.Length > 0)
            {
                this.AvailableTechs = new List<string>(this.AvailableTechs.Union(new List<string> { this.NewTech.Text }));
                this.NewTech.Text = string.Empty;
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
