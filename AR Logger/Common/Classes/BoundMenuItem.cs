namespace AR_Logger.Common.Classes
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// The bound menu item.
    /// </summary>
    public class BoundMenuItem : ObservableComponent
    {
        #region Fields

        /// <summary>
        /// Backing field.
        /// </summary>
        private ICommand command;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundMenuItem"/> class.
        /// </summary>
        /// <param name="header">The header.</param>
        public BoundMenuItem(string header)
        {
            // Initialize properties
            this.Children = new ObservableCollection<object>();
            this.Header = header;

            // Handle events
            this.Children.CollectionChanged += 
                (s, e) => this.RaisePropertyChanged(nameof(this.IsEnabled));
        }
        
        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets the MenuItem's children.
        /// </summary>
        public ObservableCollection<object> Children { get; }

        /// <summary>
        /// Gets or sets the MenuItem command.
        /// </summary>
        public ICommand Command
        {
            get
            {
                return this.command;
            }

            set
            {
                if (value != this.command)
                {
                    this.command = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(this.IsEnabled));
                }
            }
        }

        /// <summary>
        /// Gets or sets the MenuItem's icon.
        /// </summary>
        public Image Icon { get; set; }

        /// <summary>
        /// Disables menu if no Children or Command.
        /// </summary>
        public bool IsEnabled => this.Children.Count > 0 || this.Command != null;

        /// <summary>
        /// Gets or sets the MenuItem command parameter.
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Gets the MenuItem header.
        /// </summary>
        public string Header { get; }

        #endregion // Properties
    }
}
