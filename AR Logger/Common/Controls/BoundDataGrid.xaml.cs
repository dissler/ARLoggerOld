namespace AR_Logger.Common.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using AR_Logger.Common.Classes;
    using AR_Logger.Data;

    using GalaSoft.MvvmLight.Command;

    /// <summary>
    /// Interaction logic for BoundDataGrid.xaml
    /// </summary>
    public partial class BoundDataGrid : UserControl
    {
        #region Fields

        /// <summary>
        /// The display view property.
        /// </summary>
        private static readonly DependencyProperty DisplayViewProperty;

        /// <summary>
        /// The grid context menu property.
        /// </summary>
        private static readonly DependencyProperty GridContextMenuProperty;

        /// <summary>
        /// The hidden column names property.
        /// </summary>
        private static readonly DependencyProperty HiddenColumnNamesProperty;

        /// <summary>
        /// The is read only property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty;

        /// <summary>
        /// The mouse double click command property.
        /// </summary>
        private static readonly DependencyProperty MouseDoubleClickCommandProperty;

        /// <summary>
        /// The read only column names property.
        /// </summary>
        private static readonly DependencyProperty ReadOnlyColumnNamesProperty;

        /// <summary>
        /// The selected row property.
        /// </summary>
        private static readonly DependencyProperty SelectedRowProperty;

        /// <summary>
        /// The stretch column name property.
        /// </summary>
        private static readonly DependencyProperty StretchColumnNameProperty;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="BoundDataGrid"/> class.
        /// </summary>
        static BoundDataGrid()
        {
            DisplayViewProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.DisplayView),
                typeof(DataView),
                typeof(BoundDataGrid),
                new PropertyMetadata(null, OnDisplayViewChanged));
            GridContextMenuProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.GridContextMenu),
                typeof(ObservableCollection<BoundMenuItem>),
                typeof(BoundDataGrid),
                new PropertyMetadata(new ObservableCollection<BoundMenuItem>(), OnGridContextMenuChanged));
            HiddenColumnNamesProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.HiddenColumnNames),
                typeof(List<string>),
                typeof(BoundDataGrid),
                new PropertyMetadata(new List<string>(), OnDisplayViewChanged));
            IsReadOnlyProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.IsReadOnly),
                typeof(bool),
                typeof(BoundDataGrid),
                new PropertyMetadata(false, OnIsReadOnlyChanged));
            MouseDoubleClickCommandProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.MouseDoubleClickCommand),
                typeof(ICommand),
                typeof(BoundDataGrid),
                new PropertyMetadata(new RelayCommand(() => { Tools.DebugLog("Mouse clicked."); })));
            ReadOnlyColumnNamesProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.ReadOnlyColumnNames),
                typeof(List<string>),
                typeof(BoundDataGrid),
                new PropertyMetadata(new List<string>(), OnDisplayViewChanged));
            SelectedRowProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.SelectedRow),
                typeof(DataRowView),
                typeof(BoundDataGrid),
                new PropertyMetadata(null, OnSelectedRowChanged));
            StretchColumnNameProperty = DependencyProperty.Register(
                nameof(BoundDataGrid.StretchColumnName),
                typeof(string),
                typeof(BoundDataGrid),
                new PropertyMetadata(string.Empty, OnDisplayViewChanged));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundDataGrid"/> class.
        /// </summary>
        public BoundDataGrid()
        {
            this.InitializeComponent();
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the display view.
        /// </summary>
        public DataView DisplayView
        {
            get { return (DataView)this.GetValue(DisplayViewProperty); }
            set { this.SetValue(DisplayViewProperty, value); }
        }

        /// <summary>
        /// Gets or sets the context menu.
        /// </summary>
        public ObservableCollection<BoundMenuItem> GridContextMenu
        {
            get { return (ObservableCollection<BoundMenuItem>)this.GetValue(GridContextMenuProperty); }
            set { this.SetValue(GridContextMenuProperty, value); }
        }

        /// <summary>
        /// Gets or sets the hidden column names.
        /// </summary>
        public List<string> HiddenColumnNames
        {
            get { return (List<string>)this.GetValue(HiddenColumnNamesProperty); }
            set { this.SetValue(HiddenColumnNamesProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the grid is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Gets or sets the mouse double click command.
        /// </summary>
        public ICommand MouseDoubleClickCommand
        {
            get { return (ICommand)this.GetValue(MouseDoubleClickCommandProperty); }
            set { this.SetValue(MouseDoubleClickCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the read only column names.
        /// </summary>
        public List<string> ReadOnlyColumnNames
        {
            get { return (List<string>)this.GetValue(ReadOnlyColumnNamesProperty); }
            set { this.SetValue(ReadOnlyColumnNamesProperty, value); }
        }

        /// <summary>
        /// Gets or sets the selected row.
        /// </summary>
        public DataRowView SelectedRow
        {
            get { return (DataRowView)this.GetValue(SelectedRowProperty); }
            set { this.SetValue(SelectedRowProperty, value); }
        }

        /// <summary>
        /// Gets or sets the stretch column name.
        /// </summary>
        public string StretchColumnName
        {
            get { return this.GetValue(StretchColumnNameProperty).ToString(); }
            set { this.SetValue(StretchColumnNameProperty, value); }
        }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Handles changes in the display view dependency property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDisplayViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BoundDataGrid;
            if (me != null)
            {
                me.DataGrid.ItemsSource = me.DisplayView;
                me.GenerateColumns();
            }
        }

        /// <summary>
        /// Handles changes in the grid context menu dependency property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnGridContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BoundDataGrid;
            if (me?.DataGrid?.ContextMenu != null)
            {
                me.DataGrid.ContextMenu.ItemsSource = me.GridContextMenu;
            }
        }

        /// <summary>
        /// Handles changes in the is read only property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BoundDataGrid;
            if (me != null)
            {
                me.DataGrid.IsReadOnly = me.IsReadOnly;
            }
        }

        /// <summary>
        /// Handles changes in the grid selected row dependency property.
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSelectedRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = d as BoundDataGrid;
            if (me?.DataGrid != null)
            {
                me.DataGrid.SelectedItem = me.SelectedRow;
            }
        }

        /// <summary>
        /// Handles changes in the grid selected row.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedRow = this.DataGrid.SelectedItem as DataRowView;
        }

        /// <summary>
        /// Handles row load to add row number to header.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        /// <summary>
        /// Generates data grid columns for each data table column.
        /// </summary>
        private void GenerateColumns()
        {
            this.DataGrid.Columns.Clear();
            var table = this.DisplayView?.Table;
            if (table != null)
            {
                foreach (var col in table.Columns.OfType<DataColumn>()
                    .Where(c => !this.HiddenColumnNames.Contains(c.ColumnName)))
                {
                    dynamic gridCol;
                    if (col.DataType == typeof(bool))
                    {
                        // Check box column for bools
                        gridCol = new DataGridCheckBoxColumn
                        {
                            Binding = new Binding(col.ColumnName)
                        };
                    }
                    else if (col.ColumnName == LogTable.CategoryIdColumnName)
                    {
                        // Template column to bind category description and group icon
                        var imageFactory = new FrameworkElementFactory(typeof(Image));
                        imageFactory.SetValue(
                            Image.SourceProperty,
                            new Binding("CategoryId")
                            {
                                Converter = (IValueConverter)Application.Current.Resources["CategoryIdToImageConverter"]
                            });
                        imageFactory.SetValue(
                            Image.StyleProperty,
                            (Style)Application.Current.Resources["DefaultImageStyle"]);

                        gridCol = new DataGridTemplateColumn
                        {
                            CanUserSort = true,
                            CellStyle = new Style(typeof(DataGridCell))
                            {
                                Setters =
                                {
                                    new Setter(
                                    ToolTipProperty,
                                    new Binding("CategoryId")
                                    {
                                        Converter = (IValueConverter)Application.Current.Resources["CategoryIdToDescriptionConverter"]
                                    })
                                }
                            },
                            CellTemplate = new DataTemplate { VisualTree = imageFactory },
                            SortMemberPath = col.ColumnName
                        };
                    }
                    else
                    {
                        // Text column for everything else
                        gridCol = new DataGridTextColumn
                        {
                            Binding = new Binding(col.ColumnName)
                        };

                        // Set type-specific string formatting and style
                        if (col.DataType == typeof(decimal))
                        {
                            gridCol.Binding.StringFormat = "0.00";
                            gridCol.CellStyle = Application.Current.Resources["GridCellNumberStyle"] as Style;
                        }
                        else if (col.DataType == typeof(DateTime))
                        {
                            gridCol.Binding.StringFormat = "d";
                        }
                    }

                    gridCol.Header = col.Caption;
                    gridCol.IsReadOnly = this.ReadOnlyColumnNames.Contains(col.ColumnName);

                    // Set stretch column
                    if (col.ColumnName == this.StretchColumnName)
                    {
                        gridCol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    }

                    this.DataGrid.Columns.Add(gridCol);
                }
            }
        }

        #endregion // Methods
    }
}
