namespace AR_Logger.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using AR_Logger.Common;
    using AR_Logger.Data;

    using Excel = Microsoft.Office.Interop.Excel;

    /// <summary>
    /// Reads and writes Excel spreadsheets.
    /// </summary>
    internal class FileReader
    {
        #region Properties

        /// <summary>
        /// Gets the list of columns to export.
        /// </summary>
        public static List<string> ExportLogByDateColumns { get; } = new List<string>
        {
            LogTable.AccountNumColumnName,
            LogTable.CityColumnName,
            LogTable.StateColumnName,
            LogTable.ZoneColumnName,
            LogTable.AmountColumnName,
            LogTable.DoneColumnName,
            LogTable.TechColumnName,
            LogTable.NotesColumnName,
            LogTable.CategoryIdColumnName,
            LogTable.RecordIdColumnName
        };

        /// <summary>
        /// Gets the list of columns in the CSV import.
        /// </summary>
        public static List<string> ImportCsvColumns { get; } = new List<string>
        {
            LogTable.AccountNumColumnName,
            LogTable.DateColumnName,
            LogTable.AmountColumnName
        };

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Reads the given log file.
        /// </summary>
        /// <param name="filePath">The log file path.</param>
        /// <param name="progress">The progress reporter.</param>
        /// <returns>A <see cref="LogTable"/>.</returns>
        public static LogTable ReadLogFile(string filePath, IProgress<int> progress = null)
        {
            switch (Path.GetExtension(filePath))
            {
                case ".xls":
                case ".xlsx": return ReadXlFile(filePath, progress);
                default: return ReadCsvFile(filePath);
            }
        }

        /// <summary>
        /// Reads all WSRs in the given directory.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="progress">The progress reporter.</param>
        /// <returns>A <see cref="DataTable"/>.</returns>
        public static LogTable ReadWsrs(string folderPath, IProgress<int> progress = null)
        {
            // Set up import table
            var returnTable = new LogTable();
            var wsrs = Directory.GetFiles(folderPath).Where(f => Path.GetExtension(f) == Values.WsrExtension).ToList();
            var excelApp = new Excel.Application
            {
                DisplayAlerts = false,
                ScreenUpdating = false,
                Visible = true
            };
            var workbooks = excelApp.Workbooks;

            // Initialize progress reporting
            var filesRead = 0;
            var totalFiles = wsrs.Count;
            progress?.Report(0);

            foreach (var wsr in wsrs)
            {
                var thisWorkBook = workbooks.Open(wsr, ReadOnly: true);
                try
                {
                    // Get store name
                    var storeNum = Path.GetFileName(wsr)?.Split('-')[1].Trim();
                    var sheet = thisWorkBook.Sheets.OfType<Excel.Worksheet>().First().UsedRange;
                    const int DayCol = 4;
                    const int RowDate = 9;
                    const int RowAr = 45;

                    // Look for imbalances on each day
                    for (var d = 0; d < 7; d++)
                    {
                        // Check date
                        var salesDate = DateTime.Parse(sheet.Cells[RowDate, DayCol + (2 * d)].Value2.ToString());
                        if (salesDate < DateTime.Now.Date)
                        {
                            // Check for imbalance
                            var am = Convert.ToDecimal(sheet.Cells[RowAr, DayCol + (2 * d)].Value2);
                            var pm = Convert.ToDecimal(sheet.Cells[RowAr, DayCol + (2 * d) + 1].Value2);
                            if (am + pm != 0)
                            {
                                // Add row
                                var newRow = returnTable.NewRow();
                                newRow.Date = salesDate;
                                newRow.AccountNum = storeNum;
                                newRow.Amount = am + pm;
                                returnTable.Rows.Add(newRow);
                            }
                        }
                    }

                    // Report progress
                    filesRead++;
                    progress?.Report(100 * filesRead / totalFiles);
                }
                catch (Exception error)
                {
                    Tools.DebugLog(error.Message);
                }
                finally
                {
                    thisWorkBook.Close();
                    workbooks.Close();
                }
            }

            excelApp.Quit();
            return returnTable;
        }

        /// <summary>
        /// Writes a log file.
        /// </summary>
        /// <param name="reportData">The report data.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="progress">The progress reporter.</param>
        public static void WriteLogFile(LogTable reportData, string filePath, IProgress<int> progress = null)
        {
            var excelApp = new Excel.Application
            {
                DisplayAlerts = false,
                ScreenUpdating = false
            };
            var workbooks = excelApp.Workbooks;
            var thisWorkBook = workbooks.Add();

            // Initialize progress reporting
            var rowsWritten = 0;
            var totalRows = reportData.Rows.Count;
            progress?.Report(0);

            try
            {
                // Create collection of tables for worksheets
                var dateRange = reportData.Rows.OfType<LogRow>().OrderByDescending(row => row.Date)
                    .Select(row => row.Date).Distinct();
                var tables = new List<LogTable>();
                foreach (var date in dateRange)
                {
                    var newTable = new LogTable();
                    foreach (var dateRow in reportData.Rows.OfType<LogRow>().Where(row => row.Date.Date == date.Date))
                    {
                        newTable.ImportRow(dateRow);
                    }

                    tables.Add(newTable);
                }

                foreach (var thisDateReport in tables)
                {
                    // Create new worksheet
                    var thisDate = thisDateReport.FirstDate;
                    Excel.Worksheet thisSheet = thisWorkBook.Sheets.Add();
                    thisSheet.Name = $"{Tools.GetWeekDayName(thisDate)} {thisDate?.ToString("d", Values.AppCulture).Replace("/", "-")}";

                    // Format column headers
                    var headerRange = thisSheet.Range["a1"];
                    headerRange.EntireRow.Font.Bold = true;
                    headerRange.Application.ActiveWindow.SplitRow = 1;
                    headerRange.Application.ActiveWindow.FreezePanes = true;

                    // Remember datatable rows/cols are 0-indexed, xl is 1-indexed
                    for (var col = 0; col < ExportLogByDateColumns.Count; col++)
                    {
                        switch (ExportLogByDateColumns[col])
                        {
                            case LogTable.AmountColumnName:
                                thisSheet.Range[$"{GetExcelColumnName(col + 1)}1"].EntireColumn.NumberFormat =
                                    "#0.00;[Red]-#0.00";
                                break;
                            case LogTable.DoneColumnName:
                                thisSheet.Range[$"{GetExcelColumnName(col + 1)}1"]
                                    .EntireColumn.HorizontalAlignment = Excel.Constants.xlCenter;
                                break;
                            case LogTable.TechColumnName:
                                thisSheet.Range[$"{GetExcelColumnName(col + 1)}1"]
                                    .EntireColumn.HorizontalAlignment = Excel.Constants.xlCenter;
                                break;
                            default:
                                thisSheet.Range[$"{GetExcelColumnName(col + 1)}1"].EntireColumn.NumberFormat = "@";
                                break;
                        }

                        // Header contents
                        thisSheet.Cells[1, col + 1] = reportData.Columns[ExportLogByDateColumns[col]].Caption;
                    }

                    // Write rows
                    // Additional offset by 1 for header row
                    for (var row = 0; row < thisDateReport.Rows.Count; row++)
                    {
                        for (var col = 0; col < ExportLogByDateColumns.Count; col++)
                        {
                            var reportRow = thisDateReport.Rows[row] as LogRow;

                            // If the column has a bool value, use "X" and " " for true and false
                            var value = reportRow?[reportData.Columns.IndexOf(ExportLogByDateColumns[col])];
                            thisSheet.Cells[row + 2, col + 1] = thisDateReport.Columns[ExportLogByDateColumns[col]].DataType == typeof(bool)
                                ? value as bool? ?? false ? Values.BoolExportString : string.Empty
                                : value;
                        }

                        // Report progress
                        rowsWritten++;
                        progress?.Report(100 * rowsWritten / totalRows);
                    }

                    thisSheet.UsedRange.EntireColumn.AutoFit();
                }

                // Remove initial sheet(s)
                for (var s = 1; s <= excelApp.SheetsInNewWorkbook; s++)
                {
                    thisWorkBook.Sheets.OfType<Excel.Worksheet>().ToList().LastOrDefault()?.Delete();
                }

                // Save workbook
                thisWorkBook.SaveAs(filePath);
            }
            catch (Exception error)
            {
                Tools.DebugLog($"Error exporting file: {error.Message}");
            }
            finally
            {
                thisWorkBook.Close();
                workbooks.Close();
                excelApp.Quit();
            }
        }

        /// <summary>
        /// Reads the given CSV file.
        /// </summary>
        /// <param name="filePath">The CSV file path.</param>
        /// <returns>A <see cref="LogTable"/>.</returns>
        private static LogTable ReadCsvFile(string filePath)
        {
            var returnTable = new LogTable();
            returnTable.BeginLoadData();

            // AR csv report formatting
            const char Delimiter = '|';
            try
            {
                using (var readFile = new StreamReader(filePath))
                {
                    // Skip headers
                    var line = readFile.ReadLine();

                    // Get lines
                    line = readFile.ReadLine();
                    while (line != null)
                    {
                        var newLine = line.Split(Delimiter);
                        var newRow = returnTable.NewRow();
                        
                        // Parse account name, date, amount
                        newRow.AccountNum = newLine[0].Substring(0, 5).Replace("-", string.Empty).Trim();
                        newRow.Date = DateTime.Parse(newLine[1]);
                        newRow.Amount = Convert.ToDecimal(newLine[2]);

                        returnTable.Rows.Add(newRow);
                        line = readFile.ReadLine();
                    }
                }
            }
            catch (Exception error)
            {
                Tools.DebugLog($"Error reading file {filePath}: {error.Message}");
            }

            returnTable.EndLoadData();
            return returnTable;
        }

        /// <summary>
        /// Reads the given Excel file.
        /// </summary>
        /// <param name="filePath">The Excel file path.</param>
        /// <param name="progress">The progress reporter.</param>
        /// <returns>A <see cref="LogTable"/>.</returns>
        private static LogTable ReadXlFile(string filePath, IProgress<int> progress = null)
        {
            var returnTable = new LogTable();
            var excelApp = new Excel.Application
            {
                DisplayAlerts = false,
                ScreenUpdating = false,
                Visible = true
            };
            var workbooks = excelApp.Workbooks;
            returnTable.BeginLoadData();
            var thisWorkBook = workbooks.Open(filePath, ReadOnly: true);

            // Initialize progress reporting
            var rowsRead = 0;
            var totalRows = thisWorkBook.Sheets.OfType<Excel.Worksheet>().Sum(w => w.UsedRange.Rows.Count - 1); // Minus header row
            progress?.Report(0);
            
            try
            {
                foreach (var thisSheet in thisWorkBook.Sheets.OfType<Excel.Worksheet>())
                {
                    var thisRange = thisSheet.UsedRange;
                    var thisDate = Convert.ToDateTime(thisSheet.Name, Values.AppCulture);

                    // Get column headers
                    var colDict = new Dictionary<int, int>();
                    for (var col = 1; col <= thisRange.Columns.Count; col++)
                    {
                        var headerName = Convert.ToString(thisRange.Cells[1, col].Value2);
                        var colName = returnTable.Columns.OfType<DataColumn>()
                            .First(c => c.Caption == headerName || c.ColumnName == headerName).ColumnName;
                        var colIndex = returnTable.Columns.IndexOf(colName);
                        if (colIndex > 0)
                        {
                            colDict.Add(col, colIndex);
                            continue;
                        }

                        Tools.DebugLog($"Error importing column '{colName}': column not found in target table.");
                    }

                    // Read each row below the column headers
                    for (var row = 2; row <= thisRange.Rows.Count; row++)
                    {
                        var newRow = returnTable.NewRow();
                        newRow.Date = thisDate;
                        for (var col = 1; col <= thisRange.Columns.Count; col++)
                        {
                            if (colDict.ContainsKey(col))
                            {
                                // Safe casts in case cell values are null
                                var cellValue = Convert.ToString(thisRange.Cells[row, col].Value2) ?? string.Empty;
                                if (returnTable.Columns[colDict[col]].DataType == typeof(bool))
                                {
                                    newRow[colDict[col]] = cellValue.ToUpper() == Values.BoolExportString;
                                }
                                else
                                {
                                    newRow[colDict[col]] = cellValue;
                                }
                            }
                        }

                        returnTable.Add(newRow);

                        // Report progress
                        rowsRead++;
                        progress?.Report(100 * rowsRead / totalRows);
                    }
                }
            }
            catch (Exception error)
            {
                Tools.DebugLog(error.Message);
            }
            finally
            {
                thisWorkBook.Close();
                workbooks.Close();
            }

            excelApp.Quit();
            returnTable.EndLoadData();
            return returnTable;
        }

       /// <summary>
        /// Gets the excel column name from the 1-based column index.
        /// Adapted from http://stackoverflow.com/questions/181596/how-to-convert-a-column-number-eg-127-into-an-excel-column-eg-aa
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The excel column name.</returns>
        private static string GetExcelColumnName(int index)
        {
            var dividend = index;
            var columnName = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = $"{Convert.ToChar('A' + modulo)}{columnName}";
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        #endregion // Methods
    }
}
