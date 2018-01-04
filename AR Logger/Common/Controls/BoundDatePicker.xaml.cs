namespace AR_Logger.Common.Controls
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for BoundDatePicker.xaml
    /// </summary>
    public partial class BoundDatePicker : UserControl, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The first date property.
        /// </summary>
        private static readonly DependencyProperty FirstDateProperty;

        /// <summary>
        /// The last date property.
        /// </summary>
        private static readonly DependencyProperty LastDateProperty;

        /// <summary>
        /// The selected date property.
        /// </summary>
        private static readonly DependencyProperty SelectedDateProperty;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="BoundDatePicker"/> class.
        /// </summary>
        static BoundDatePicker()
        {
            FirstDateProperty = DependencyProperty.Register(
                nameof(BoundDatePicker.FirstDate),
                typeof(DateTime?),
                typeof(BoundDatePicker),
                new PropertyMetadata(null, OnDatePropertyChanged));
            LastDateProperty = DependencyProperty.Register(
                nameof(BoundDatePicker.LastDate),
                typeof(DateTime?),
                typeof(BoundDatePicker),
                new PropertyMetadata(null, OnDatePropertyChanged));
            SelectedDateProperty = DependencyProperty.Register(
                nameof(BoundDatePicker.SelectedDate),
                typeof(DateTime?),
                typeof(BoundDatePicker),
                new PropertyMetadata(null, OnDatePropertyChanged));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundDatePicker"/> class.
        /// </summary>
        public BoundDatePicker()
        {
            this.InitializeComponent();

            // Initialize properties
            this.DatePicker.FirstDayOfWeek = Tools.GetWeekStart();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the first date.
        /// </summary>
        public DateTime? FirstDate
        {
            get
            {
                return (DateTime?)this.GetValue(FirstDateProperty);
            }

            set
            {
                if (value != (DateTime?)this.GetValue(FirstDateProperty))
                {
                    this.SetValue(FirstDateProperty, value);
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the last date.
        /// </summary>
        public DateTime? LastDate
        {
            get
            {
                return (DateTime?)this.GetValue(LastDateProperty);
            }

            set
            {
                if (value != (DateTime?)this.GetValue(LastDateProperty))
                {
                    this.SetValue(LastDateProperty, value);
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        public DateTime? SelectedDate
        {
            get
            {
                return (DateTime?)this.GetValue(SelectedDateProperty);
            }

            set
            {
                if (value != (DateTime?)this.GetValue(SelectedDateProperty))
                {
                    this.SetValue(SelectedDateProperty, value);
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Handles changes in date dependency properties.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BoundDatePicker;
            if (me?.DatePicker != null)
            {
                switch (e.Property.Name)
                {
                    case nameof(me.FirstDate):
                        me.DatePicker.DisplayDateStart = me.FirstDate;
                        if (me.SelectedDate != null && me.SelectedDate < me.FirstDate)
                        {
                            me.SelectedDate = me.FirstDate;
                        }

                        break;
                    case nameof(me.LastDate):
                        me.DatePicker.DisplayDateEnd = me.LastDate;
                        if (me.SelectedDate == null)
                        {
                            me.SelectedDate = me.LastDate;
                        }
                        else if (me.SelectedDate > me.LastDate)
                        {
                            me.SelectedDate = me.LastDate;
                        }

                        break;
                    case nameof(me.SelectedDate):
                        if (e.OldValue == null || e.NewValue != null)
                        {
                            me.DatePicker.SelectedDate = me.SelectedDate;

                            // Configure labels
                            me.DayLabel.Text = me.SelectedDate != null
                                ? $@"{Tools.GetWeekDayName((DateTime)me.SelectedDate)}"
                                : string.Empty;
                            me.WeekLabel.Text = me.SelectedDate != null
                                ? $@"{me.SelectedDate?.Year} Week {Tools.GetWeekNumber((DateTime)me.SelectedDate)}"
                                : string.Empty;
                        }
                        else
                        {
                            me.SelectedDate = (DateTime)e.OldValue;
                        }
                        
                        break;
                }
            }
        }

        /// <summary>
        /// Handles changes in the date picker's selected date.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedDate = this.DatePicker.SelectedDate;
        }

        #endregion // Methods

        #region INotify

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotify
    }
}
