namespace AR_Logger.Data
{
    using System;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// The ticket table.
    /// </summary>
    public class TicketTable : DataTable
    {
        #region Fields

        /// <summary>
        /// The account ID column name.
        /// </summary>
        public const string AccountIdColumnName = "AccountId";

        /// <summary>
        /// The ticket column name.
        /// </summary>
        public const string TicketNumColumnName = "TicketNum";

        /// <summary>
        /// The creating date column name.
        /// </summary>
        public const string CreatedColumnName = "Created";

        /// <summary>
        /// The notes column name.
        /// </summary>
        public const string NotesColumnName = "Notes";

        /// <summary>
        /// The notes column default value.
        /// </summary>
        public const string NotesColumnDefaultValue = "";

        /// <summary>
        /// The active column name.
        /// </summary>
        public const string ActiveColumnName = "Active";

        /// <summary>
        /// The active column default value.
        /// </summary>
        public const bool ActiveColumnDefaultValue = true;

        /// <summary>
        /// The ticket ID column name.
        /// </summary>
        public const string TicketIdColumnName = "TicketId";

        /// <summary>
        /// The ticket id column default value.
        /// </summary>
        public const int TicketIdColumnDefaultValue = 0;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketTable"/> class.
        /// </summary>
        public TicketTable()
        {
            // Initialize properties
            this.Columns.AddRange(new[]
            {
                new DataColumn(AccountIdColumnName, typeof(int)),
                new DataColumn(TicketNumColumnName, typeof(string)) { Caption = "Ticket #", DefaultValue = TicketIdColumnDefaultValue },
                new DataColumn(CreatedColumnName, typeof(DateTime)) { DefaultValue = DateTime.Now },
                new DataColumn(NotesColumnName, typeof(string)) { DefaultValue = NotesColumnDefaultValue },
                new DataColumn(ActiveColumnName, typeof(bool)) { DefaultValue = ActiveColumnDefaultValue },
                new DataColumn(TicketIdColumnName, typeof(int)) { DefaultValue = TicketIdColumnDefaultValue }
            });
        }

        #endregion // Constructors

        #region Properties

        #endregion // Properties

        #region Methods
        /// <summary>
        /// Adds a row.
        /// </summary>
        /// <param name="row">The row.</param>
        public void Add(TicketRow row)
        {
            this.Rows.Add(row);
        }

        /// <summary>
        /// Returns the number of rows.
        /// </summary>
        /// <returns>An <see cref="int"/>.</returns>
        public int Count()
        {
            return this.Rows.OfType<TicketRow>().Count();
        }

        /// <summary>
        /// Creates a new row.
        /// </summary>
        /// <returns>A <see cref="TicketRow"/>.</returns>
        public new TicketRow NewRow()
        {
            return (TicketRow)base.NewRow();
        }

        /// <summary>
        /// Removes a row.
        /// </summary>
        /// <param name="row">The row.</param>
        public void Remove(TicketRow row)
        {
            this.Rows.Remove(row);
        }

        /// <summary>
        /// Gets the row type.
        /// </summary>
        /// <returns>A <see cref="Type"/>.</returns>
        protected override Type GetRowType()
        {
            return typeof(TicketRow);
        }

        /// <summary>
        /// Builds a new row.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>A <see cref="DataRow"/>.</returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new TicketRow(builder);
        }

        #endregion // Methods
    }

    /// <summary>
    /// The ticket row.
    /// </summary>
    public class TicketRow : DataRow
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketRow"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public TicketRow(DataRowBuilder builder) : base(builder)
        {
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the account ID.
        /// </summary>
        public int AccountId
        {
            get { return (int)base[TicketTable.AccountIdColumnName]; }
            set { base[TicketTable.AccountIdColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the ticket number.
        /// </summary>
        public string TicketNum
        {
            get { return (string)base[TicketTable.TicketNumColumnName]; }
            set { base[TicketTable.TicketNumColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime Created
        {
            get { return (DateTime)base[TicketTable.CreatedColumnName]; }
            set { base[TicketTable.CreatedColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string Notes
        {
            get { return (string)base[TicketTable.NotesColumnName]; }
            set { base[TicketTable.NotesColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ticket is active.
        /// </summary>
        public bool Active
        {
            get { return (bool)base[TicketTable.ActiveColumnName]; }
            set { base[TicketTable.ActiveColumnName] = value; }
        }

        /// <summary>
        /// Gets or sets the ticket ID.
        /// </summary>
        public int TicketId
        {
            get { return (int)base[TicketTable.TicketIdColumnName]; }
            set { base[TicketTable.TicketIdColumnName] = value; }
        }

        #endregion // Properties
    }
}
