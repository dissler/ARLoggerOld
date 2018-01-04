namespace AR_Logger.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows;

    using AR_Logger.Common;
    using AR_Logger.Properties;

    /// <summary>
    /// Holds transaction-specific SQL tools.
    /// </summary>
    public class TransactionTools
    {
        #region Fields

        /// <summary>
        /// The default id.
        /// </summary>
        private const long DefaultId = 9999;

        /// <summary>
        /// The select statement to get the entity id.
        /// </summary>
        private static readonly string EntityId = $@"(SELECT StoreID FROM {Settings.Default.TransDbName}.dbo.tbStoreInfo)";

        /// <summary>
        /// The table name.
        /// </summary>
        private static readonly string TableName = $@"{Settings.Default.TransDbName}.dbo.tbSalesMain";

        /// <summary>
        /// Data table for formatting.
        /// </summary>
        private static readonly DataTable Format;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes static members of the <see cref="TransactionTools"/> class.
        /// </summary>
        static TransactionTools()
        {
            // Initialize properties
            Format = new DataTable();
            Format.Columns.AddRange(new[]
            {
                new DataColumn("RecordType") { DataType = typeof(string), DefaultValue = "H" },
                new DataColumn("TransactionID"),
                new DataColumn("RecordSubType") { DataType = typeof(string) },
                new DataColumn("EntityID"),
                new DataColumn("RegisterID") { DataType = typeof(int), DefaultValue = 1 },
                new DataColumn("PollDate") { DataType = typeof(string) },
                new DataColumn("PollCount") { DataType = typeof(int), DefaultValue = 1 },
                new DataColumn("PollAmount") { DataType = typeof(decimal), DefaultValue = 0 },
                new DataColumn("ClerkID") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("ClerkName") { DataType = typeof(string), DefaultValue = string.Empty },
                new DataColumn("CustomerID") { DataType = typeof(string), DefaultValue = string.Empty },
                new DataColumn("Synced") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("PLUCodeID") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("PLUCode") { DataType = typeof(string), DefaultValue = string.Empty },
                new DataColumn("SequenceNo") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("Flag") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("ApplyTax") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("ItemTax") { DataType = typeof(decimal), DefaultValue = 0 },
                new DataColumn("PriceLevel") { DataType = typeof(string), DefaultValue = "0" },
                new DataColumn("DateAdded") { DataType = typeof(string) },
                new DataColumn("SubTypeDescription"),
                new DataColumn("POSTransactionID") { DataType = typeof(int), DefaultValue = DefaultId },
                new DataColumn("TransactionVersion") { DataType = typeof(int), DefaultValue = 0 },
                new DataColumn("SyncToken")
            });

            CardTypes = new List<string> { "AMEX", "Disc", "M/C", "Visa" };
            MidTypes = new List<string> { "'Brick And Mortar'", "'MOTO'", "'ECOMMERCE'" };
            Registers = new List<int> { 1, 2, 3, 4, 5, 33 };

            CreditTemplates = new List<DataRow>();
            foreach (var card in CardTypes)
            {
                var template = Format.NewRow();
                template["RecordType"] = "F";
                template["RecordSubType"] = "P";
                template["PLUCode"] = $"CredCard{card}";
                CreditTemplates.Add(template);
            }

            NonCreditTemplates = new List<DataRow>();
            var nonCredit = new List<string[]>
            {
                new[] { "I", string.Empty, "254", "Delivery Charge", "0" },
                new[] { "F", "D", "0", "%Employee Discount", "0" },
                new[] { "F", "D", "0", "%Manager Discount", "0" },
                new[] { "F", "D", "0", "%Promo", "0" },
                new[] { "F", "D", "0", "Sampling", "0" },
                new[] { "F", "P", "0", "Cash", string.Empty }
            };
            foreach (var trans in nonCredit)
            {
                var template = Format.NewRow();
                template["RecordType"] = trans[0];
                template["RecordSubType"] = trans[1];
                template["PLUCodeID"] = Convert.ToInt64(trans[2]);
                template["PLUCode"] = trans[3];
                template["PriceLevel"] = trans[4];
                NonCreditTemplates.Add(template);
            }

            RowTemplates = new List<DataRow>();
            RowTemplates.AddRange(CreditTemplates);
            RowTemplates.AddRange(NonCreditTemplates);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the credit transaction templates list.
        /// </summary>
        public static List<DataRow> CreditTemplates { get; set; }

        /// <summary>
        /// Gets or sets the non-credit transaction templates list.
        /// </summary>
        public static List<DataRow> NonCreditTemplates { get; set; }

        /// <summary>
        /// Gets or sets the full list of transaction templates.
        /// </summary>
        public static List<DataRow> RowTemplates { get; set; }

        /// <summary>
        /// Gets or sets the card types.
        /// </summary>
        public static List<string> CardTypes { get; set; }

        /// <summary>
        /// Gets or sets the merchant ID types.
        /// </summary>
        public static List<string> MidTypes { get; set; }

        /// <summary>
        /// Gets or sets the list of register numbers.
        /// </summary>
        public static List<int> Registers { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The generates a default transaction adjustment.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>A <see cref="bool"/> indicating success.</returns>
        public static string GenerateDefaultTrans(LogRow row)
        {
            try
            {
                var transId = GenerateTransId();
                var header = Format.NewRow();
                var template = RowTemplates.First(p => p["PLUCode"].ToString() == "Delivery Charge");
                template["PollAmount"] = row.Amount;
                var colList = Format.Columns.OfType<DataColumn>().Where(col => col.ColumnName != "SalesMainID").Aggregate(
                    string.Empty,
                    (current, col) =>
                        $"{current}{(string.IsNullOrEmpty(current) ? string.Empty : ", ")}[{col.ColumnName}]");

                var insertText =
                    $"IF (SELECT COUNT(1) FROM {TableName} WHERE TransactionID = {transId}) = 0 BEGIN"
                    + $"\nINSERT INTO {TableName} ({colList}) \nSELECT {GenerateStatementColumns(header, row.Date, transId)} "
                    + $"\nUNION \nSELECT {GenerateStatementColumns(template, row.Date, transId)}; "
                    + $"\nSELECT 'Transaction inserted' as [Success] END \nELSE "
                    + $"\nSELECT 'TransactionID already exists in {TableName}! Use another TransactionID.' AS [Error]; "
                    + $"\nSELECT * FROM {TableName} WHERE TransactionID = {transId} ORDER BY SalesMainID";
                
                return insertText;
            }
            catch (Exception error)
            {
                Tools.DebugLog($"Error generating transaction: {error.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates a list of values for each DataColumn in tbSalesMain.
        /// </summary>
        /// <param name="row">The template DataRow.</param>
        /// <param name="date">The date.</param>
        /// <param name="transId">The trans id.</param>
        /// <returns>The full statement.</returns>
        public static string GenerateStatementColumns(DataRow row, DateTime date, string transId)
        {
            var returnText = string.Empty;

            for (var col = 0; col < Format.Columns.Count - 1; col++)
            {
                if (Format.Columns[col].ColumnName == "SalesMainID")
                {
                    continue;
                }

                if (Format.Columns[col].ColumnName == "DateAdded")
                {
                    returnText += $"'{date}'";
                }
                else if (Format.Columns[col].ColumnName == "PollDate")
                {
                    returnText += $"'{date.ToString("dd-MMM-yyyy hh:mm:ss", Values.AppCulture)}'";
                }
                else if (Format.Columns[col].ColumnName == "TransactionID")
                {
                    returnText += $"{transId}";
                }
                else if (Format.Columns[col].ColumnName == "EntityID")
                {
                    returnText += $"{EntityId}";
                }
                else if (Format.Columns[col].DataType == typeof(decimal) || Format.Columns[col].DataType == typeof(int))
                {
                    returnText += $"{row[col]}";
                }
                else
                {
                    returnText += string.IsNullOrEmpty(row[col].ToString()) ? "NULL" : $"'{row[col]}'";
                }

                returnText += ", ";
            }

            returnText += string.IsNullOrEmpty(row[Format.Columns.Count - 1].ToString())
                ? "NULL"
                : $"'{row[Format.Columns.Count - 1]}'";

            return returnText;
        }

        /// <summary>
        /// Generates a random transaction id.
        /// </summary>
        /// <returns>The transaction id.</returns>
        public static string GenerateTransId()
        {
            var rand = new Random(DateTime.Now.Millisecond);
            return $"{DefaultId}{rand.Next(0, (int)Math.Pow(10, 7))}{DefaultId}";
        }

        #endregion
    }
}
