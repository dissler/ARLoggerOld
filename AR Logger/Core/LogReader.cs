namespace AR_Logger.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using AR_Logger.Common;
    using AR_Logger.Common.Classes;
    using AR_Logger.Data;
    using AR_Logger.Properties;

    /// <summary>
    /// Handles interactions with the AR log database.
    /// </summary>
    internal class LogReader : ObservableComponent
    {
        #region Fields

        /// <summary>
        /// The remote log connection string.
        /// </summary>
        private readonly string connectionString;

        #region Backing Fields

        private bool isConnected;

        private Exception lastException;

        private string remoteLogName;

        #endregion // Backing fields

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReader"/> class.
        /// </summary>
        public LogReader()
        {
            // Initialize fields
            this.connectionString = Debugger.IsAttached
                ? Settings.Default.ARLogDevConnectionString
                : Settings.Default.ARLogConnectionString;
        }

        #endregion // Constructors

        #region Properties

        #region Status

        /// <summary>
        /// Gets or sets a value indicating whether there is a connection to the remote database.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.isConnected;
            }

            set
            {
                if (value != this.IsConnected)
                {
                    this.isConnected = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the last remote connection attempt.
        /// </summary>
        public DateTime LastConnectionAttempt { get; private set; }

        /// <summary>
        /// Gets or sets the last exception.
        /// </summary>
        public Exception LastException
        {
            get
            {
                return this.lastException;
            }

            set
            {
                if (value != this.lastException)
                {
                    this.lastException = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the remote log name.
        /// </summary>
        public string RemoteLogName
        {
            get
            {
                return this.remoteLogName;
            }

            set
            {
                if (value != this.remoteLogName)
                {
                    this.remoteLogName = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion // Status

        #region Stored Procedures - No Parameters

        /// <summary>
        /// Gets the name of the get all account details stored procedure.
        /// </summary>
        public string SpGetAccountDetails { get; } = "sp_GetAccountDetails";

        /// <summary>
        /// Gets the name of the get categories stored procedure.
        /// </summary>
        public string SpGetCategories { get; } = "sp_GetCategories";

        /// <summary>
        /// Gets the name of the get date range stored procedure.
        /// </summary>
        public string SpGetDateRange { get; } = "sp_GetDateRange";

        /// <summary>
        /// Gets the name of the get tech list stored procedure.
        /// </summary>
        public string SpGetTechList { get; } = "sp_GetTechList";

        #endregion // Stored Procedures - No Parameters

        #region Stored Procedures - One Parameter

        /// <summary>
        /// Gets the name of the get log by account stored procedure.
        /// </summary>
        public string SpLogByAccount { get; } = "sp_GetLogByAccount";

        /// <summary>
        /// Gets the list of parameters for the get log by date range stored procedure.
        /// </summary>
        public List<string[]> SpLogByAccountParams { get; } = new List<string[]>
        {
            new[] { "@AccountId", LogTable.AccountIdColumnName }
        };

        /// <summary>
        /// Gets the name of the get tickets by account stored procedure.
        /// </summary>
        public string SpTicketsByAccount { get; } = "sp_GetTickets";

        /// <summary>
        /// Gets the list of parameters for the get tickets by date range stored procedure.
        /// </summary>
        public List<string[]> SpTicketsByAccountParams { get; } = new List<string[]>
        {
            new[] { "@AccountId", TicketTable.AccountIdColumnName }
        };

        #endregion // Stored Procedures - One Parameter

        #region Stored Procedures

        /// <summary>
        /// Gets the name of the get log by date range stored procedure.
        /// </summary>
        public string SpLogByDate { get; } = "sp_GetLogByDate";

        /// <summary>
        /// Gets the list of parameters for the get log by date range stored procedure.
        /// </summary>
        public List<string> SpLogByDateParams { get; } = new List<string>
        {
            "@firstDate",
            "@lastDate"
        };

        /// <summary>
        /// Gets the name of the send log by date stored procedure.
        /// </summary>
        public string SpUpsertLogByDate { get; } = "sp_UpsertLogByDate";

        /// <summary>
        /// Gets the list of parameters for the send log by date range stored procedure.
        /// </summary>
        public List<string[]> SpUpsertLogByDateParamsIn { get; } = new List<string[]>
        {
            new[] { "@RecordId", LogTable.RecordIdColumnName },
            new[] { "@AccountName", LogTable.AccountNameColumnName },
            new[] { "@AccountNum", LogTable.AccountNumColumnName },
            new[] { "@BusDate", LogTable.DateColumnName },
            new[] { "@Amount", LogTable.AmountColumnName },
            new[] { "@Done", LogTable.DoneColumnName },
            new[] { "@Tech", LogTable.TechColumnName },
            new[] { "@Notes", LogTable.NotesColumnName },
            new[] { "@CategoryId", LogTable.CategoryIdColumnName }
        };

        /// <summary>
        /// Gets the output parameter for the send log by date range stored procedure.
        /// </summary>
        public string SpUpsertLogByDateParamOut { get; } = "@NewRecordId";

        // TODO: upsert account details, tickets

        #endregion // Stored Procedures

        #endregion // Properties

        #region Methods

        #region No Parameters

        /// <summary>
        /// Gets the list of account details.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<LogTable> GetAccountDetailsAsync()
        {
            var resultTable = new LogTable();
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpGetAccountDetails,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };
                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultTable);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Gets the list of available categories.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<DataTable> GetCategoriesAsync()
        {
            var resultTable = new DataTable();
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpGetCategories,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };
                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultTable);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Gets the log's date range.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<DataTable> GetDateRangeAsync()
        {
            var resultTable = new DataTable();
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpGetDateRange,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };
                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultTable);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Gets the list of technicians.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<DataTable> GetTechListAsync()
        {
            var resultTable = new DataTable();
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpGetTechList,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };
                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultTable);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Tests the connection to the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<bool> TestConnectionAsync()
        {
            var returnValue = false;
            using (var conn = new SqlConnection(this.connectionString))
            {
                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    returnValue = true;
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    Tools.DebugLog(error.Message);
                    this.SetStatusDisconnected(error);
                }
            }

            return returnValue;
        }

        #endregion // No Parameters

        #region One Parameter

        /// <summary>
        /// Async log query by the selected account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<LogTable> GetLogByAccountAsync(int accountId)
        {
            var resultTable = new LogTable();
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpLogByAccount,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };

                // Add input parameters and values
                foreach (var parameter in this.SpLogByAccountParams)
                {
                    cmd.Parameters.AddWithValue(parameter[0], accountId);
                }

                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultTable);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Async ticket query by the selected account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<TicketTable> GetTicketsAsync(int accountId)
        {
            var resultTable = new TicketTable();
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpTicketsByAccount,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };

                // Add input parameters and values
                foreach (var parameter in this.SpTicketsByAccountParams)
                {
                    cmd.Parameters.AddWithValue(parameter[0], accountId);
                }

                try
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(resultTable);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }
            return resultTable;
        }

        #endregion // One Parameter

        #region Multiple Parameters

        /// <summary>
        /// Async query by the selected date.
        /// </summary>
        /// <param name="firstDate">The first date.</param>
        /// <param name="lastDate">The last date.</param>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<LogTable> GetLogByDateAsync(DateTime? firstDate, DateTime? lastDate = null)
        {
            var resultsTable = new LogTable();
            if (firstDate != null)
            {
                using (var conn = new SqlConnection(this.connectionString))
                {
                    var cmd = new SqlCommand
                    {
                        CommandText = this.SpLogByDate,
                        CommandType = CommandType.StoredProcedure,
                        Connection = conn
                    };
                    cmd.Parameters.AddWithValue(this.SpLogByDateParams[0], firstDate);
                    if (lastDate != null)
                    {
                        cmd.Parameters.AddWithValue(this.SpLogByDateParams[1], lastDate);
                    }

                    try
                    {
                        await conn.OpenAsync().ConfigureAwait(false);
                        var adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(resultsTable);
                        this.SetStatusConnected();
                    }
                    catch (Exception error)
                    {
                        this.SetStatusDisconnected(error);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return resultsTable;
        }

        /// <summary>
        /// Sends a new or updated row to the database.
        /// </summary>
        /// <param name="row">The log row.</param>
        /// <returns>A <see cref="Task{TResult}"/>.</returns>
        public async Task<int> UpsertLogByDateAsync(LogRow row)
        {
            var returnValue = -1;
            using (var conn = new SqlConnection(this.connectionString))
            {
                var cmd = new SqlCommand
                {
                    CommandText = this.SpUpsertLogByDate,
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };

                // Add input parameters and values
                foreach (var parameter in this.SpUpsertLogByDateParamsIn)
                {
                    cmd.Parameters.AddWithValue(parameter[0], row[parameter[1]]);
                }

                // Add output parameter
                cmd.Parameters.AddWithValue(
                    this.SpUpsertLogByDateParamOut,
                    SqlDbType.BigInt).Direction = ParameterDirection.Output;
                
                try
                {
                    await conn.OpenAsync().ConfigureAwait(true);
                    cmd.ExecuteNonQuery();
                    returnValue = Convert.ToInt32(cmd.Parameters[this.SpUpsertLogByDateParamOut].Value);
                    this.SetStatusConnected();
                }
                catch (Exception error)
                {
                    this.SetStatusDisconnected(error);
                }
                finally
                {
                    conn.Close();
                }
            }

            return returnValue;
        }

        #endregion // Multiple Parameters

        /// <summary>
        /// Sets the status to connected.
        /// </summary>
        private void SetStatusConnected()
        {
            // Perform changes, then set IsConnected
            this.LastConnectionAttempt = DateTime.Now;
            using (var conn = new SqlConnection(this.connectionString))
            {
                this.RemoteLogName = $@"{conn.DataSource}\{conn.Database}";
            }

            this.LastException = null;
            this.IsConnected = true;
        }

        /// <summary>
        /// Sets the status to disconnected.
        /// </summary>
        /// <param name="exception">The exception</param>
        private void SetStatusDisconnected(Exception exception = null)
        {
            // Perform changes, then set IsConnected
            this.LastConnectionAttempt = DateTime.Now;
            this.RemoteLogName = string.Empty;
            this.LastException = exception;
            this.IsConnected = false;
        }

        #endregion // Methods
    }
}
