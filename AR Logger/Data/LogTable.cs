namespace AR_Logger.Data
{
    using System;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// The log by date table.
    /// </summary>
    public class LogTable : DataTable
    {
        #region Fields

        /// <summary>
        /// The import column name.
        /// </summary>
        public const string ImportColumnName = "Import";

        /// <summary>
        /// The import column default value.
        /// </summary>
        public const bool ImportColumnDefaultValue = true;

        /// <summary>
        /// The date column name.
        /// </summary>
        public const string DateColumnName = "Date";

        /// <summary>
        /// The account name column name.
        /// </summary>
        public const string AccountNameColumnName = "AccountName";

        /// <summary>
        /// The account name column default value.
        /// </summary>
        public const string AccountNameColumnDefaultValue = "Jimmy John's";

        /// <summary>
        /// The account number column name.
        /// </summary>
        public const string AccountNumColumnName = "AccountNum";

        /// <summary>
        /// The street column name.
        /// </summary>
        public const string StreetColumnName = "Street";

        /// <summary>
        /// Gets the city column name.
        /// </summary>
        public const string CityColumnName = "City";

        /// <summary>
        /// The state column name.
        /// </summary>
        public const string StateColumnName = "State";

        /// <summary>
        /// Gets the zone column name.
        /// </summary>
        public const string ZoneColumnName = "Zone";

        /// <summary>
        /// The amount column name.
        /// </summary>
        public const string AmountColumnName = "Amount";

        /// <summary>
        /// The done column name.
        /// </summary>
        public const string DoneColumnName = "Done";

        /// <summary>
        /// The done column default value.
        /// </summary>
        public const bool DoneColumnDefaultValue = false;

        /// <summary>
        /// The tech column name.
        /// </summary>
        public const string TechColumnName = "Tech";

        /// <summary>
        /// The tech column default value.
        /// </summary>
        public const string TechColumnDefaultValue = "";

        /// <summary>
        /// The notes column name.
        /// </summary>
        public const string NotesColumnName = "Notes";

        /// <summary>
        /// The notes column default value.
        /// </summary>
        public const string NotesColumnDefaultValue = "";

        /// <summary>
        /// The category ID column name.
        /// </summary>
        public const string CategoryIdColumnName = "CategoryId";

        /// <summary>
        /// The category id column default value.
        /// </summary>
        public const int CategoryIdColumnDefaultValue = 0;

        /// <summary>
        /// The account ID column name.
        /// </summary>
        public const string AccountIdColumnName = "AccountId";

        /// <summary>
        /// The account id column default value.
        /// </summary>
        public const int AccountIdColumnDefaultValue = 0;

        /// <summary>
        /// The record ID column name.
        /// </summary>
        public const string RecordIdColumnName = "RecordId";

        /// <summary>
        /// The record id column default value.
        /// </summary>
        public const int RecordIdColumnDefaultValue = 0;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTable"/> class.
        /// </summary>
        public LogTable()
        {
            // Initialize properties
            this.Columns.AddRange(new[]
            {
                new DataColumn(ImportColumnName, typeof(bool)) { DefaultValue = ImportColumnDefaultValue },
                new DataColumn(DateColumnName, typeof(DateTime)) { DefaultValue = DateTime.MinValue },
                new DataColumn(AccountNameColumnName, typeof(string)) { DefaultValue = AccountNameColumnDefaultValue },
                new DataColumn(AccountNumColumnName, typeof(string)) { Caption = "Account #", DefaultValue = string.Empty },
                new DataColumn(StreetColumnName, typeof(string)) { DefaultValue = string.Empty },
                new DataColumn(CityColumnName, typeof(string)) { DefaultValue = string.Empty },
                new DataColumn(StateColumnName, typeof(string)) { DefaultValue = string.Empty },
                new DataColumn(ZoneColumnName, typeof(string)) { DefaultValue = string.Empty },
                new DataColumn(AmountColumnName, typeof(decimal)) { DefaultValue = 0 },
                new DataColumn(DoneColumnName, typeof(bool)) { DefaultValue = DoneColumnDefaultValue },
                new DataColumn(TechColumnName, typeof(string)) { DefaultValue = TechColumnDefaultValue },
                new DataColumn(NotesColumnName, typeof(string)) { DefaultValue = NotesColumnDefaultValue },
                new DataColumn(CategoryIdColumnName, typeof(int)) { Caption = "Category", DefaultValue = CategoryIdColumnDefaultValue },
                new DataColumn(AccountIdColumnName, typeof(int)) { DefaultValue = AccountIdColumnDefaultValue },
                new DataColumn(RecordIdColumnName, typeof(int)) { DefaultValue = RecordIdColumnDefaultValue }
            });
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets the earliest date in the data table.
        /// </summary>
        public DateTime? FirstDate => (DateTime?)(this.Rows.Count > 0
            ? (ValueType)this.Rows.OfType<LogRow>().Select(row => row.Date).Min() 
            : null);

        /// <summary>
        /// Gets the latest date in the data table.
        /// </summary>
        public DateTime? LastDate => (DateTime?)(this.Rows.Count > 0 
            ? (ValueType)this.Rows.OfType<LogRow>().Select(row => row.Date).Max()
            : null);

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Adds a row.
        /// </summary>
        /// <param name="row">The row.</param>
        public void Add(LogRow row)
        {
            this.Rows.Add(row);
        }

        /// <summary>
        /// Returns the number of rows.
        /// </summary>
        /// <returns>An <see cref="int"/>.</returns>
        public int Count()
        {
            return this.Rows.OfType<LogRow>().Count();
        }

        /// <summary>
        /// Creates a new row.
        /// </summary>
        /// <returns>A <see cref="LogRow"/>.</returns>
        public new LogRow NewRow()
        {
            return (LogRow)base.NewRow();
        }

        /// <summary>
        /// Removes a row.
        /// </summary>
        /// <param name="row">The row.</param>
        public void Remove(LogRow row)
        {
            this.Rows.Remove(row);
        }

        /// <summary>
        /// Gets the row type.
        /// </summary>
        /// <returns>A <see cref="Type"/>.</returns>
        protected override Type GetRowType()
        {
            return typeof(LogRow);
        }

        /// <summary>
        /// Builds a new row.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>A <see cref="DataRow"/>.</returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new LogRow(builder);
        }

        #endregion // Methods
    }

    /// <summary>
    /// The log by date row.
    /// </summary>
    public class LogRow : DataRow
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRow"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public LogRow(DataRowBuilder builder) : base(builder)
        {
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the record is selected for import.
        /// </summary>
        public bool Import
        {
            get { return (bool)base[LogTable.ImportColumnName]; }
            set { base[LogTable.ImportColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date
        {
            get { return (DateTime)base[LogTable.DateColumnName]; }
            set { base[LogTable.DateColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account name.
        /// </summary>
        public string AccountName
        {
            get { return (string)base[LogTable.AccountNameColumnName]; }
            set { base[LogTable.AccountNameColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        public string AccountNum
        {
            get { return (string)base[LogTable.AccountNumColumnName]; }
            set { base[LogTable.AccountNumColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account street.
        /// </summary>
        public string Street
        {
            get { return (string)base[LogTable.StreetColumnName]; }
            set { base[LogTable.StreetColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account city.
        /// </summary>
        public string City
        {
            get { return (string)base[LogTable.CityColumnName]; }
            set { base[LogTable.CityColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account state.
        /// </summary>
        public string State
        {
            get { return (string)base[LogTable.StateColumnName]; }
            set { base[LogTable.StateColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account zone.
        /// </summary>
        public string Zone
        {
            get { return (string)base[LogTable.ZoneColumnName]; }
            set { base[LogTable.ZoneColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount
        {
            get { return (decimal)base[LogTable.AmountColumnName]; }
            set { base[LogTable.AmountColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the record is done.
        /// </summary>
        public bool Done
        {
            get { return (bool)base[LogTable.DoneColumnName]; }
            set { base[LogTable.DoneColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the assigned technician.
        /// </summary>
        public string Tech
        {
            get { return (string)base[LogTable.TechColumnName]; }
            set { base[LogTable.TechColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string Notes
        {
            get { return (string)base[LogTable.NotesColumnName]; }
            set { base[LogTable.NotesColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
        public int CategoryId
        {
            get { return (int)base[LogTable.CategoryIdColumnName]; }
            set { base[LogTable.CategoryIdColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the account ID.
        /// </summary>
        public int AccountId
        {
            get { return (int)base[LogTable.AccountIdColumnName]; }
            set { base[LogTable.AccountIdColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the record ID.
        /// </summary>
        public int RecordId
        {
            get { return (int)base[LogTable.RecordIdColumnName]; }
            set { base[LogTable.RecordIdColumnName] = value; }
        }

        #endregion // Properties
    }
}
