namespace AR_Logger.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using AR_Logger.Common;
    using AR_Logger.Common.Classes;
    using AR_Logger.Data;
    using AR_Logger.Dialogs;
    using AR_Logger.Properties;
    using AR_Logger.Windows;

    using GalaSoft.MvvmLight.Command;

    using Microsoft.Win32;

    using WinForms = System.Windows.Forms;

    /// <summary>
    /// The main app logic.
    /// </summary>
    internal class Main : ObservableComponent
    {
        #region Fields

        /// <summary>
        /// The remote log reader.
        /// </summary>
        private readonly LogReader logReader;

        /// <summary>
        /// The list of accounts that have been edited offline.
        /// </summary>
        private readonly List<int> offlineEditAccounts;

        /// <summary>
        /// The list of records that have been edited offline.
        /// </summary>
        private readonly List<int> offlineEditRecords;

        /// <summary>
        /// The list of tickets that have been edited offline.
        /// </summary>
        private readonly List<int> offlineEditTickets;

        /// <summary>
        /// The account history window.
        /// </summary>
        private AccountHistoryWindow accountHistoryWindow;

        /// <summary>
        /// The adjust dialog location.
        /// </summary>
        private Point adjustDialogLocation;

        /// <summary>
        /// Whether to suspend sending row changes to the remote log.
        /// Used to prevent uploading rows that were just downloaded.
        /// </summary>
        private bool suspendRemoteSync;

        /// <summary>
        /// Whether to suspend view refreshing.
        /// </summary>
        private bool suspendViewRefresh;

        /// <summary>
        /// The tech list.
        /// </summary>
        private Dictionary<string, DateTime> techList;

        #region Backing Fields

        /// <summary>
        /// Backing field.
        /// </summary>
        private BoundMenuItem categoryList;

        /// <summary>
        /// Backing fields.
        /// </summary>
        private DateTime? firstLogDate, lastLogDate, selectedLogDate;

        /// <summary>
        /// Backing fields.
        /// </summary>
        private bool isRemoteConnection, showProgress;

        /// <summary>
        /// Backing field.
        /// </summary>
        private ObservableCollection<BoundMenuItem> logContextMenu;

        /// <summary>
        /// Backing fields.
        /// </summary>
        private int progress;

        /// <summary>
        /// Backing fields.
        /// </summary>
        private string remoteLogName, statusText, statusTimeStamp;

        /// <summary>
        /// Backing fields.
        /// </summary>
        private DataRowView selectedLogView, selectedAccountLogView;

        /// <summary>
        /// Backing field.
        /// </summary>
        private LogRow selectedAccountRow;

        #endregion // Backing Fields

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Main"/> class.
        /// </summary>
        internal Main()
        {
            // Initialize components
            this.adjustDialogLocation = new Point(100, 100);
            this.offlineEditAccounts = new List<int>();
            this.offlineEditRecords = new List<int>();
            this.offlineEditTickets = new List<int>();
            this.techList = new Dictionary<string, DateTime>();
            this.CancelFileAsync = new CancellationToken(); // TODO: actually figure out how this thing works
            this.CancelSqlAsync = new CancellationToken();

            // Initialize data tables
            this.AccountsTable = new LogTable();
            this.AccountsTable.RowChanged += this.OnAccountDetailsRowChanged;
            this.LocalLog = new LogTable();
            this.LocalLog.RowChanged += this.OnLogRowChanged;
            this.TicketTable = new TicketTable();
            this.TicketTable.RowChanged += this.OnTicketLogRowChanged;

            // Initialize data views
            this.AccountLogView = new DataView(this.LocalLog);
            this.AccountsView = this.AccountsTable.DefaultView;
            this.LocalLogView = new DataView(this.LocalLog);
            this.TicketView = this.TicketTable.DefaultView;

            // Initialize grid context menu
            this.CategoryMenu = new BoundMenuItem("Set Category");
            this.LogContextMenu = new ObservableCollection<BoundMenuItem>
            {
                new BoundMenuItem("Adjust Transaction...")
                {
                    Command = new RelayCommand(this.AdjustTransAmount),
                    Icon = (Image)Application.Current.Resources["IconMoney"]
                },
                new BoundMenuItem("Generate Transaction")
                {
                    Command = new RelayCommand(this.GenerateTransAmount),
                    Icon = (Image)Application.Current.Resources["IconGenTrans"]
                },
                this.CategoryMenu
            };

            // Initialize view filters
            this.ViewFilters = new Dictionary<string, string>
            {
                { LogTable.DateColumnName, string.Empty },
                { LogTable.DoneColumnName, string.Empty },
                { LogTable.TechColumnName, string.Empty },
                { LogTable.ZoneColumnName, string.Empty }
            };
            this.ViewMenus = new ObservableCollection<BoundMenuItem>
            {
                new BoundMenuItem(LogTable.DoneColumnName),
                new BoundMenuItem(LogTable.TechColumnName),
                new BoundMenuItem(LogTable.ZoneColumnName)
            };

            // Handle events
            this.PropertyChanged += this.OnPropertyChanged;

            // Test remote connection
            this.logReader = new LogReader();
            this.logReader.PropertyChanged += this.OnLogReaderPropertyChanged;
            this.logReader.TestConnectionAsync();
        }

        #endregion // Constructors

        #region Properties

        #region Components

        /// <summary>
        /// Gets or sets the cancellation token for file async tasks.
        /// </summary>
        public CancellationToken CancelFileAsync { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for SQL async tasks.
        /// </summary>
        public CancellationToken CancelSqlAsync { get; set; }

        /// <summary>
        /// The progress reporter for file operations.
        /// </summary>
        public IProgress<int> ProgressReporter => new Progress<int>(p => this.Progress = p);

        /// <summary>
        /// The synchronization context.
        /// </summary>
        public TaskScheduler SyncContext => SynchronizationContext.Current != null
            ? TaskScheduler.FromCurrentSynchronizationContext()
            : TaskScheduler.Default;

        #endregion // Components

        #region Menus and Status Bar

        /// <summary>
        /// Whether there are unassigned cases.
        /// </summary>
        public bool AreUnassignedCases => this.LocalLog.Rows.OfType<LogRow>()
            .Any(row => string.IsNullOrEmpty(row.Tech));
        
        /// <summary>
        /// Gets or sets a value indicating whether there is a remote connection.
        /// </summary>
        public bool IsRemoteConnection
        {
            get
            {
                return this.isRemoteConnection;
            }

            set
            {
                if (value != this.isRemoteConnection)
                {
                    this.isRemoteConnection = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        public int Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                if (value != this.Progress)
                {
                    this.progress = value;
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

        /// <summary>
        /// Gets or sets a value indicating whether to show the progress bar.
        /// </summary>
        public bool ShowProgress
        {
            get
            {
                return this.showProgress;
            }

            set
            {
                if (value != this.showProgress)
                {
                    this.showProgress = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (value != this.statusText)
                {
                    this.statusText = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the status time stamp.
        /// </summary>
        public string StatusTimeStamp
        {
            get
            {
                return this.statusTimeStamp;
            }

            set
            {
                if (value != this.statusTimeStamp)
                {
                    this.statusTimeStamp = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the current user's initials.
        /// </summary>
        public string UserInitials => Tools.GetUserInitials();

        #endregion // Menus and Status Bar

        #region Log Handling

        /// <summary>
        /// Gets or sets the local log by date table.
        /// </summary>
        public LogTable LocalLog { get; set; }

        /// <summary>
        /// Gets or sets the local log by date view.
        /// </summary>
        public DataView LocalLogView { get; set; }

        /// <summary>
        /// Gets or sets the first log date.
        /// </summary>
        public DateTime? FirstLogDate
        {
            get
            {
                return this.firstLogDate;
            }

            set
            {
                if (value != this.firstLogDate)
                {
                    this.firstLogDate = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the last log date.
        /// </summary>
        public DateTime? LastLogDate
        {
            get
            {
                return this.lastLogDate;
            }

            set
            {
                if (value != this.lastLogDate)
                {
                    this.lastLogDate = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected log date.
        /// </summary>
        public DateTime? SelectedLogDate
        {
            get
            {
                return this.selectedLogDate;
            }

            set
            {
                if (value != this.selectedLogDate)
                {
                    this.selectedLogDate = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected log row view.
        /// </summary>
        public DataRowView SelectedLogView
        {
            get
            {
                return this.selectedLogView;
            }

            set
            {
                if (!object.Equals(value, this.selectedLogView))
                {
                    this.selectedLogView = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the selected log by date row.
        /// </summary>
        public LogRow SelectedLogRow => this.SelectedLogView?.Row as LogRow;

        #endregion // Log Handling

        #region Log Display

        /// <summary>
        /// Gets or sets the record category menu.
        /// </summary>
        public BoundMenuItem CategoryMenu
        {
            get
            {
                return this.categoryList;
            }

            set
            {
                if (value != this.categoryList)
                {
                    this.categoryList = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the log context menu.
        /// </summary>
        public ObservableCollection<BoundMenuItem> LogContextMenu
        {
            get
            {
                return this.logContextMenu;
            }

            set
            {
                if (value != this.logContextMenu)
                {
                    this.logContextMenu = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the view filters.
        /// </summary>
        public Dictionary<string, string> ViewFilters { get; set; }

        /// <summary>
        /// Gets or sets the view filter description.
        /// </summary>
        public string ViewFilterDescription =>
            this.ViewFilters.Any(f => f.Key != LogTable.DateColumnName && !string.IsNullOrEmpty(f.Value))
                ? $"Viewing: {this.ViewFilters.Where(f => f.Key != LogTable.DateColumnName && !string.IsNullOrEmpty(f.Value)).Aggregate(string.Empty, (current, next) => $"{current}{(!string.IsNullOrEmpty(current) ? ", " : string.Empty)}{next.Key} = {next.Value}")}"
                : string.Empty;

        /// <summary>
        /// Gets or sets the view menus.
        /// </summary>
        public ObservableCollection<BoundMenuItem> ViewMenus { get; set; }

        #endregion // Log Display

        #region Account Handling

        /// <summary>
        /// Gets or sets the account details table.
        /// </summary>
        public LogTable AccountsTable { get; set; }

        /// <summary>
        /// Gets or sets the accounts view.
        /// </summary>
        public DataView AccountsView { get; set; }

        /// <summary>
        /// Gets or sets the local log by account view.
        /// </summary>
        public DataView AccountLogView { get; set; }

        /// <summary>
        /// Gets or sets the selected account log view.
        /// </summary>
        public DataRowView SelectedAccountLogView
        {
            get
            {
                return this.selectedAccountLogView;
            }

            set
            {
                if (!object.Equals(value, this.selectedAccountLogView))
                {
                    this.selectedAccountLogView = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected account row.
        /// </summary>
        public LogRow SelectedAccountRow
        {
            get
            {
                return this.selectedAccountRow;
            }

            set
            {
                if (!object.Equals(value, this.selectedAccountRow))
                {
                    this.selectedAccountRow = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the selected account log row.
        /// </summary>
        public LogRow SelectedAccountLogRow => this.SelectedAccountLogView?.Row as LogRow;

        /// <summary>
        /// Gets or sets the ticket table.
        /// </summary>
        public TicketTable TicketTable { get; set; }

        /// <summary>
        /// Gets or sets the ticket view.
        /// </summary>
        public DataView TicketView { get; set; }

        #endregion // Account Handling

        #endregion // Properties

        #region Commands

        #region File

        /// <summary>
        /// The import command.
        /// </summary>
        public ICommand ImportCommand => new RelayCommand(this.ImportLogFile);

        /// <summary>
        /// The import from WSRs command.
        /// </summary>
        public ICommand ImportWsrsCommand => new RelayCommand(this.ImportWsrFiles);

        /// <summary>
        /// The export command.
        /// </summary>
        public ICommand ExportCommand => new RelayCommand<string>(this.ExportLogByDate);

        /// <summary>
        /// The exit app command.
        /// </summary>
        public ICommand ExitAppCommand => new RelayCommand(() =>
        {
            Application.Current.Shutdown();
        });

        #endregion // File

        #region View

        /// <summary>
        /// The clear filters command.
        /// </summary>
        public ICommand ClearFiltersCommand => new RelayCommand(() =>
        {
            var filters = this.ViewFilters.Keys.Where(k => k != LogTable.DateColumnName).ToList();
            foreach (var filter in filters)
            {
                this.ViewFilters[filter] = string.Empty;
            }

            this.RefreshLocalView();
        });

        /// <summary>
        /// The set filters command.
        /// </summary>
        public ICommand SetFiltersCommand => new RelayCommand<string>(p =>
        {
            var filter = p.Split(',');
            if (this.ViewFilters.ContainsKey(filter[0]))
            {
                this.ViewFilters[filter[0]] = filter.Length > 1 ? filter[1] : string.Empty;
                this.RefreshLocalView();
            }
        });

        #endregion // View

        #region Tools

        /// <summary>
        /// The assign cases command.
        /// </summary>
        public ICommand AssignCasesCommand => new RelayCommand(() =>
        {
            // Get list of recent techs
            // TODO: For now get techs active within the last month, could move this logic to the dialog and add a toggle
            var availableTechs = this.techList.Where(tech => DateTime.Now.Subtract(tech.Value).Days <= 30)
                .Select(tech => tech.Key).ToList();
            if (!this.techList.Any())
            {
                availableTechs.AddRange(new[] { "PM", "DK" });
            }

            var assignDialog = new AssignCasesDialog(availableTechs, this.SelectedLogDate);
            if (assignDialog.ShowDialog() ?? false)
            {
                // Get list of unassigned cases for the selected date, sorted by amount
                var unassignedRows = this.LocalLog.Rows.OfType<LogRow>()
                        .Where(row => row.Date == this.SelectedLogDate && string.IsNullOrEmpty(row.Tech))
                        .OrderBy(row => row.Amount).ToList();
                var techs = assignDialog.SelectedTechs;

                // Starting w/ a random tech, cycle through the list until all cases for the day are assigned
                var rand = new Random(DateTime.Now.Millisecond);
                var nextTech = rand.Next(0, techs.Count);

                foreach (var row in unassignedRows)
                {
                    row.Tech = techs[nextTech];
                    nextTech = nextTech == techs.Count - 1 ? 0 : nextTech + 1;
                }
            }
        });

        /// <summary>
        /// The create ticket command.
        /// </summary>
        public ICommand TicketCommand => new RelayCommand(() =>
        {
        });

        #endregion // Tools

        #region Window

        /// <summary>
        /// The show account report command.
        /// </summary>
        public ICommand AccountCommand => new RelayCommand(() =>
        {
            // Get account log
            if (this.SelectedLogRow != null)
            {
                this.SelectedAccountRow = this.AccountsTable.Rows.OfType<LogRow>()
                    .First(row => row.AccountId == this.SelectedLogRow.AccountId);
            }

            // Open and focus on account history window
            if (this.accountHistoryWindow == null)
            {
                this.accountHistoryWindow = new AccountHistoryWindow
                {
                    DataContext = this
                };
                this.accountHistoryWindow.Closing += (s, e) =>
                {
                    e.Cancel = true;
                    this.accountHistoryWindow.Visibility = Visibility.Hidden;
                };
            }

            this.accountHistoryWindow.Visibility = Visibility.Visible;
            this.accountHistoryWindow.Focus();
        });

        /// <summary>
        /// The show account manager command.
        /// </summary>
        public ICommand AccountManagerCommand => new RelayCommand(() =>
        {
        });

        /// <summary>
        /// The script library command.
        /// </summary>
        public ICommand LibraryCommand => new RelayCommand(() =>
        {
        });

        /// <summary>
        /// The go to record command.
        /// </summary>
        public ICommand GoToRecordCommand => new RelayCommand(() =>
        {
            // Based on selected account history row, focus on main window,
            // select row.Date, scroll to row.RecordId
        });

        #endregion // Window

        #region Help

        /// <summary>
        /// The about command.
        /// </summary>
        public ICommand AboutCommand => new RelayCommand(() =>
        {
            var about = new AboutDialog();
            about.ShowDialog();
        });

        #endregion // Help

        #region Buttons

        /// <summary>
        /// The cancel button command.
        /// </summary>
        public ICommand CancelButtonCommand => new RelayCommand(() =>
        {
            // Cancel current async task
        });

        /// <summary>
        /// The create new ticket command.
        /// </summary>
        public ICommand NewTicketCommand => new RelayCommand(() =>
        {
            // Open create ticket dialog, pass selected account if any
        });

        /// <summary>
        /// The sync button command.
        /// </summary>
        public ICommand SyncButtonCommand => new RelayCommand(this.FullRemoteSync);

        #endregion // Buttons

        #endregion // Commands

        #region Methods

        #region File Import and Export

        /// <summary>
        /// Exports the log by date range.
        /// </summary>
        /// <param name="dateRangeOption">The date range option.</param>
        private async void ExportLogByDate(string dateRangeOption)
        {
            // Determine date range to export
            var title = "Export ";
            string suggestedName;
            DateTime exportFirstDate;
            DateTime exportLastDate;
            var exportData = new LogTable();

            // Get formatted date strings
            var displayDay = Tools.GetWeekDayName(this.SelectedLogDate);
            var displayWeek = Tools.GetWeekNumber(this.SelectedLogDate);
            var displayYear = ((DateTime)this.SelectedLogDate).Year;

            if (dateRangeOption == Resources.MainMenuExportRange)
            {
                var rangeDialog = new DateRangeDialog();
                if ((rangeDialog.ShowDialog() ?? false) && rangeDialog.FirstDate != null && rangeDialog.LastDate != null)
                {
                    exportFirstDate = (DateTime)rangeDialog.FirstDate;
                    exportLastDate = (DateTime)rangeDialog.LastDate;
                    title += $"{exportFirstDate:d} - {exportLastDate:d}";
                    suggestedName = $"{exportFirstDate.Year}Range{exportFirstDate:MMdd}-{exportLastDate:MMdd}-00.xlsx";
                }
                else
                {
                    return;
                }
            }
            else if (dateRangeOption == Resources.MainMenuExportWeek)
            {
                // Export selected week
                title += $"{displayYear} Week {displayWeek}";
                suggestedName = $"{displayYear}Week{displayWeek}-00.xlsx";
                var dateRange = Tools.GetDatesByWeekNum(
                    Tools.GetWeekNumber(this.SelectedLogDate),
                    this.SelectedLogDate?.Year);
                exportFirstDate = dateRange.Min();
                exportLastDate = dateRange.Max();
            }
            else
            {
                // Default: export selected day
                title += $"{displayDay} {this.SelectedLogDate:d}";
                suggestedName = $"{displayYear}Week{displayWeek}-{displayDay}-{this.SelectedLogDate:MMdd}-00.xlsx";
                exportFirstDate = (DateTime)this.SelectedLogDate;
                exportLastDate = (DateTime)this.SelectedLogDate;
            }

            // If we have a remote connection, request log for date range and get updated data
            if (this.isRemoteConnection)
            {
                var result = this.logReader.GetLogByDateAsync(exportFirstDate, exportLastDate).Result;
                this.ImportRemoteLogByDate(result);
            }
            
            foreach (var row in this.LocalLog.Rows.OfType<LogRow>()
                .Where(row => row.Date >= exportFirstDate && row.Date <= exportLastDate))
            {
                exportData.ImportRow(row);
            }

            if (exportData.Rows.Count == 0)
            {
                this.SetStatusText("Export: no data for selected date(s).");
                return;
            }

            // Prompt for file name
            var saveFile = new SaveFileDialog
            {
                Title = title,
                DefaultExt = ".xlsx",
                FileName = suggestedName,
                Filter = "Imbalance Report|*.xlsx|All Files|*.*"
            };
            if (saveFile.ShowDialog() ?? false)
            {
                this.SetStatusText($"Exporting '{Path.GetFileName(saveFile.FileName)}'...");
                this.ShowProgress = true;
                await Task.Run(() => FileReader.WriteLogFile(exportData, saveFile.FileName, this.ProgressReporter), this.CancelFileAsync).ContinueWith(
                    task =>
                    {
                        this.SetStatusText("Export complete.");
                        this.ShowProgress = false;
                    }, 
                    this.CancelFileAsync,
                    TaskContinuationOptions.None,
                    this.SyncContext);
            }
        }

        /// <summary>
        /// Imports a log file.
        /// </summary>
        private void ImportLogFile()
        {
            var openFile = new OpenFileDialog
            {
                Title = "Import file",
                Filter = "Imbalance Report|*.csv;*.xls;*.xlsx|All Files|*.*"
            };
            if (openFile.ShowDialog() ?? false)
            {
                this.SetStatusText($"Importing {Path.GetFileName(openFile.FileName)}...");
                this.ShowProgress = true;
                Task.Run(() => FileReader.ReadLogFile(openFile.FileName, this.ProgressReporter), this.CancelFileAsync).ContinueWith(
                    task => this.PromptToImportLog(task.Result),
                    this.CancelFileAsync,
                    TaskContinuationOptions.None,
                    this.SyncContext);
            }
        }

        /// <summary>
        /// Imports data from a folder of WSR files.
        /// </summary>
        private void ImportWsrFiles()
        {
            var openFolder = new WinForms.FolderBrowserDialog
            {
                Description = @"Select folder",
                ShowNewFolderButton = false
            };
            if (openFolder.ShowDialog() == WinForms.DialogResult.OK)
            {
                this.SetStatusText($"Importing WSRs from '{openFolder.SelectedPath}'...");
                this.ShowProgress = true;
                Task.Run(() => FileReader.ReadWsrs(openFolder.SelectedPath, this.ProgressReporter), this.CancelFileAsync).ContinueWith(
                    task => this.PromptToImportLog(task.Result), 
                    this.CancelFileAsync,
                    TaskContinuationOptions.None,
                    this.SyncContext);
            }
        }

        /// <summary>
        /// Prompts the user to import records into the local log.
        /// </summary>
        /// <param name="importTable">The import table.</param>
        private void PromptToImportLog(LogTable importTable)
        {
            var import = new ImportDialog(importTable);
            if (import.ShowDialog() ?? false)
            {
                var importRows = importTable.Rows.OfType<LogRow>().Where(row => row.Import).ToList();
                this.ImportLogByDate(importRows);
            }

            this.SetStatusText("Import complete.");
            this.ShowProgress = false;
        }

        #endregion // File Import and Export

        #region Sql Import and Export

        /// <summary>
        /// Queries remote log for all parameters.
        /// </summary>
        private void FullRemoteSync()
        {
            if (this.IsRemoteConnection)
            {
                // Send any new or edited accounts, records or tickets
                this.SendPendingEdits();

                // Get date range and log for the selected date
                this.QuickRemoteSync();

                // Get account, category, and tech lists
                this.GetRemoteAccountDetails();
                this.GetRemoteCategories();
                this.GetRemoteTechList();
            }
            else
            {
                this.logReader.TestConnectionAsync();
            }
        }

        /// <summary>
        /// Queries the remote log for date range and log for selected date.
        /// </summary>
        private void QuickRemoteSync()
        {
            this.GetRemoteDateRange();
            this.GetRemoteLog();
        }
        
        /// <summary>
        /// Gets all account details.
        /// </summary>
        private void GetRemoteAccountDetails()
        {
            if (this.IsRemoteConnection)
            {
                this.suspendRemoteSync = true;

                var accountDetails = this.logReader.GetAccountDetailsAsync().Result;
                this.AccountsTable.Rows.Clear();
                foreach (var row in accountDetails.Rows.OfType<LogRow>())
                {
                    this.AccountsTable.ImportRow(row);
                }
                
                this.suspendRemoteSync = false;
            }
        }

        /// <summary>
        /// Gets the list of log categories from the remote log.
        /// </summary>
        private void GetRemoteCategories()
        {
            if (this.IsRemoteConnection)
            {
                var categories = this.logReader.GetCategoriesAsync().Result.Rows.OfType<DataRow>().ToList();
                var groups = categories.Select(row => row[2].ToString()).Distinct().OrderBy(row => row).ToList();

                // Add each submenu rather than generating new collection so that property changes are triggered
                this.CategoryMenu.Children.Clear();
                foreach (var group in groups)
                {
                    var groupMenu = new BoundMenuItem(group);
                    if (Values.CategoryMenuIcons.ContainsKey(group))
                    {
                        groupMenu.Icon = new Image { Source = Values.CategoryMenuIcons[group] };
                    }

                    foreach (var match in categories.Where(row => row[2].ToString() == group))
                    {
                        // Add category id to dictionary for description and image lookup
                        var categoryId = Convert.ToInt32(match[0]);
                        var categoryDesc = match[1].ToString();
                        if (!Values.CategoryDescriptions.ContainsKey(categoryId))
                        {
                            Values.CategoryDescriptions.Add(categoryId, categoryDesc);
                        }

                        if (!Values.CategoryMenuIcons.ContainsKey(categoryId.ToString()))
                        {
                            Values.CategoryMenuIcons.Add(categoryId.ToString(), Values.CategoryMenuIcons[group]);
                        }

                        groupMenu.Children.Add(new BoundMenuItem(categoryDesc)
                        {
                            Command = new RelayCommand<string>(this.SetRecordCategory),
                            Parameter = categoryId.ToString()
                        });
                    }

                    this.CategoryMenu.Children.Add(groupMenu);
                }

                this.CategoryMenu.Children.Add(new Separator());
                var allCategories = new BoundMenuItem("All");
                foreach (var category in categories)
                {
                    allCategories.Children.Add(new BoundMenuItem(category[1].ToString())
                    {
                        Command = new RelayCommand<string>(this.SetRecordCategory),
                        Parameter = category[0].ToString()
                    });
                }

                this.CategoryMenu.Children.Add(allCategories);
                this.CategoryMenu.Children.Add(new BoundMenuItem("None")
                {
                    Command = new RelayCommand<string>(this.SetRecordCategory),
                    Parameter = "0"
                });
            }
        }

        /// <summary>
        /// Gets the remote log date range.
        /// </summary>
        private void GetRemoteDateRange()
        {
            if (this.IsRemoteConnection)
            {
                var dateQuery = this.logReader.GetDateRangeAsync().Result;
                if (dateQuery.Rows.Count > 0)
                {
                    var range = dateQuery.Rows.OfType<DataRow>().First();
                    var firstDate = (DateTime)range[0];
                    var lastDate = (DateTime)range[1];

                    // If local log table has rows, set date range to widest available
                    this.FirstLogDate = this.LocalLog.Count() > 0
                        ? new[] { firstDate, (DateTime)this.LocalLog.FirstDate }.Min()
                        : firstDate;
                    this.LastLogDate = this.LocalLog.Count() > 0
                        ? new[] { lastDate, (DateTime)this.LocalLog.LastDate }.Max()
                        : lastDate;
                    Tools.DebugLog($"Got date range {this.FirstLogDate:d} - {this.LastLogDate:d}.");
                }
                else
                {
                    this.FirstLogDate = this.LocalLog.FirstDate;
                    this.LastLogDate = this.LocalLog.LastDate;
                }
            }
        }

        /// <summary>
        /// Gets the remote log by account for the specified account.
        /// </summary>
        /// <param name="accountId">The account ID, default is currently selected account.</param>
        private void GetRemoteLogByAccount(int? accountId = null)
        {
            if (this.IsRemoteConnection && this.SelectedAccountRow?.AccountId != null)
            {
                this.ImportRemoteLogByDate(
                    this.logReader.GetLogByAccountAsync(accountId ?? this.SelectedAccountRow.AccountId).Result);
            }
        }

        /// <summary>
        /// Gets the remote log by date for the specified date range.
        /// </summary>
        /// <param name="firstDate">The first date, default is the selected date.</param>
        /// <param name="lastDate">The last date, default is the first date.</param>
        private void GetRemoteLog(DateTime? firstDate = null, DateTime? lastDate = null)
        {
            if (this.IsRemoteConnection)
            {
                this.ImportRemoteLogByDate(this.logReader.GetLogByDateAsync(firstDate ?? this.SelectedLogDate, lastDate).Result);
            }
        }

        /// <summary>
        /// Gets the list of log categories from the remote log.
        /// </summary>
        private void GetRemoteTechList()
        {
            if (this.IsRemoteConnection)
            {
                this.techList = this.logReader.GetTechListAsync().Result.Rows.OfType<DataRow>().ToDictionary(
                    row => row[0].ToString(),
                    row => Convert.ToDateTime(row[1]));
            }
        }

        /// <summary>
        /// Gets the tickets for the specified account.
        /// </summary>
        /// <param name="accountId">The account ID, default is currently selected account.</param>
        private void GetRemoteTickets(int? accountId = null)
        {
            if (this.IsRemoteConnection && this.SelectedAccountRow?.AccountId != null)
            {
                Task.Run(
                    () => this.logReader.GetTicketsAsync(accountId ?? this.SelectedAccountRow.AccountId),
                    this.CancelSqlAsync).ContinueWith(
                    task =>
                    {
                        if (task.Result.Count() > 0)
                        {
                            this.suspendRemoteSync = true;
                            var tickets = task.Result.Rows.OfType<TicketRow>();
                            foreach (var ticket in tickets)
                            {
                                var oldTickets = this.TicketTable.Rows.OfType<TicketRow>()
                                    .Where(row => row.TicketId == ticket.TicketId).ToList();
                                if (oldTickets.Any())
                                {
                                    foreach (var oldTicket in oldTickets)
                                    {
                                        oldTicket.Active = ticket.Active;
                                        oldTicket.Notes = ticket.Notes;
                                        oldTicket.TicketNum = ticket.TicketNum;
                                    }
                                }
                                else
                                {
                                    this.TicketTable.ImportRow(ticket);
                                }
                            }

                            this.suspendRemoteSync = false;
                        }
                    },
                    this.CancelSqlAsync,
                    TaskContinuationOptions.None,
                    this.SyncContext);
            }
        }

        /// <summary>
        /// Sends any pending edits.
        /// </summary>
        private void SendPendingEdits()
        {
            // Send edited rows
            var pendingRows = this.LocalLog.Rows.OfType<LogRow>()
                .Where(row => this.offlineEditRecords.Contains(row.RecordId)
                    || row.RecordId == LogTable.RecordIdColumnDefaultValue).ToList();
            foreach (var row in pendingRows)
            {
                Task.Run(() => this.logReader.UpsertLogByDateAsync(row), this.CancelSqlAsync).ContinueWith(
                    task =>
                    {
                        if (task.Result > 0)
                        {
                            this.offlineEditRecords.Remove(row.RecordId);
                        }
                    },
                    this.CancelSqlAsync,
                    TaskContinuationOptions.None,
                    this.SyncContext);
            }

            // Send edited account details
            var pendingAccounts = this.AccountsTable.Rows.OfType<LogRow>()
                .Where(row => this.offlineEditAccounts.Contains(row.AccountId)).ToList();
            foreach (var account in pendingAccounts)
            {
                // send edits
            }

            // Send edited tickets
            var pendingTickets = this.TicketTable.Rows.OfType<TicketRow>()
                .Where(row => this.offlineEditTickets.Contains(row.TicketId)).ToList();
            foreach (var ticket in pendingTickets)
            {
                // send edits
            }
        }

        /// <summary>
        /// Handles remote status.
        /// </summary>
        /// <param name="isConnected">Whether we have a remote connection.</param>
        private void SetRemoteStatus(bool isConnected)
        {
            if (isConnected)
            {
                this.IsRemoteConnection = true;
                this.RemoteLogName = this.logReader.RemoteLogName;
                this.StatusText = "Connected to remote log.";
                this.FullRemoteSync();
            }
            else
            {
                this.IsRemoteConnection = false;
                this.RemoteLogName = "Disconnected";
                this.StatusText = "Offline.";
            }
        }

        #endregion // Sql Import and Export

        #region Local Log Import

        /// <summary>
        /// Imports the remote log table into the local log table.
        /// </summary>
        /// <param name="importTable">The table to import.</param>
        private void ImportRemoteLogByDate(LogTable importTable)
        {
            this.ImportLogByDate(importTable.Rows.OfType<LogRow>(), false);
        }

        /// <summary>
        /// Imports an enumerable of log by date rows into the local log table. After importing, refreshes the UI state.
        /// </summary>
        /// <param name="importRows">The rows to import.</param>
        /// <param name="updateRemote">Whether to also send imported rows to the database.</param>
        private void ImportLogByDate(IEnumerable<LogRow> importRows, bool updateRemote = true)
        {
            // Suspend refreshing the data view while we're importing
            this.suspendViewRefresh = true;
            this.suspendRemoteSync = !updateRemote;
            foreach (var newRow in importRows)
            {
                // Find any matching row to update
                var oldRows = this.LocalLog.Rows.OfType<LogRow>().Where(
                    row => (row.RecordId != LogTable.RecordIdColumnDefaultValue && row.RecordId == newRow.RecordId)
                           || (row.Date == newRow.Date && row.AccountNum == newRow.AccountNum
                               && row.Amount == newRow.Amount)).ToList();
                if (oldRows.Count > 1)
                {
                    Tools.DebugLog($"Error: multiple local rows match imported row {newRow.Date:d}, {newRow.AccountNum}, {newRow.Amount}.");
                }
                else if (oldRows.Count < 1)
                {
                    this.LocalLog.ImportRow(newRow);
                }
                else if (newRow.Done != LogTable.DoneColumnDefaultValue
                    || newRow.Tech != LogTable.TechColumnDefaultValue
                    || newRow.Notes != LogTable.NotesColumnDefaultValue
                    || newRow.CategoryId != LogTable.CategoryIdColumnDefaultValue)
                {
                    // Unless new row has default done, tech, notes, and category, update values
                    var oldRow = oldRows.First();
                    if (oldRow.RecordId == LogTable.RecordIdColumnDefaultValue)
                    {
                        oldRow.RecordId = newRow.RecordId;
                    }

                    // Update account data
                    if (!string.IsNullOrEmpty(newRow.City))
                    {
                        oldRow.City = newRow.City;
                    }

                    if (!string.IsNullOrEmpty(newRow.State))
                    {
                        oldRow.State = newRow.State;
                    }

                    if (!string.IsNullOrEmpty(newRow.Zone))
                    {
                        oldRow.Zone = newRow.Zone;
                    }

                    // Update record data
                    oldRow.Amount = newRow.Amount;
                    oldRow.Done = newRow.Done;
                    oldRow.Tech = newRow.Tech;
                    oldRow.Notes = newRow.Notes;
                    oldRow.CategoryId = newRow.CategoryId;
                }
            }

            // Adjust available date range
            if (this.FirstLogDate == null || this.FirstLogDate > this.LocalLog.FirstDate)
            {
                this.FirstLogDate = this.LocalLog.FirstDate;
            }

            if (this.LastLogDate == null || this.LastLogDate < this.LocalLog.LastDate)
            {
                this.LastLogDate = this.LocalLog.LastDate;
            }

            // If we're importing from a log file, set selected date to latest date
            if (updateRemote)
            {
                this.SelectedLogDate = this.LocalLog.LastDate;
            }

            this.suspendRemoteSync = false;
            this.suspendViewRefresh = false;
            this.RefreshLocalView();
        }

        #endregion // Local Log Import

        #region Grid Interaction

        /// <summary>
        /// Opens a dialog to adjust a transaction amount.
        /// </summary>
        private void AdjustTransAmount()
        {
            var adjustDialog = new AdjustDialog
            {
                Left = this.adjustDialogLocation.X,
                SelectedLogRow = this.SelectedLogRow,
                Top = this.adjustDialogLocation.Y
            };
            if (adjustDialog.ShowDialog() ?? false)
            {
                this.adjustDialogLocation = new Point(adjustDialog.Left, adjustDialog.Top);

                // Update row
                decimal newAmount;
                decimal oldAmount;
                if (decimal.TryParse(adjustDialog.NewAmount.Text, out newAmount)
                    && decimal.TryParse(adjustDialog.OldAmount.Text, out oldAmount))
                {
                    this.SelectedLogRow.Notes +=
                        $"{(string.IsNullOrEmpty(this.SelectedLogRow.Notes) ? string.Empty : " ")}" +
                        $"Transaction {adjustDialog.TransactionId.Text} {oldAmount:n2} => {newAmount:n2}.";
                    Clipboard.SetText($"{newAmount:n2}");
                    this.SetStatusText($"Amount '{newAmount:n2}' copied to clipboard.");
                }
                
                this.SelectedLogRow.Done = true;
                int adjustCategoryId;
                if (int.TryParse(Settings.Default.AdjustTransCategoryId, out adjustCategoryId))
                {
                    this.SelectedLogRow.CategoryId = adjustCategoryId;
                } 
            }
        }

        /// <summary>
        /// Generates a default transaction for the selected row.
        /// </summary>
        private void GenerateTransAmount()
        {
            if (this.SelectedLogRow != null)
            {
                var genTrans = TransactionTools.GenerateDefaultTrans(this.SelectedLogRow);
                if (!string.IsNullOrEmpty(genTrans))
                {
                    Clipboard.SetText(genTrans);
                    this.SetStatusText("Transaction copied to clipboard.");
                }
            }
        }

        /// <summary>
        /// Sets the selected row's category.
        /// </summary>
        /// <param name="category">The category.</param>
        private void SetRecordCategory(object category)
        {
            var categoryId = Convert.ToInt32(category);
            if (this.SelectedLogRow != null)
            {
                this.SelectedLogRow.CategoryId = categoryId;
            }
        }

        #endregion // Grid Interaction

        #region View Elements

        /// <summary>
        /// Generates a row filter for the log view from the view filters.
        /// </summary>
        private void GenerateLogViewFilter()
        {
            var rowFilter = this.ViewFilters.Where(p => !string.IsNullOrEmpty(p.Value)).Aggregate(
                string.Empty,
                (current, next) =>
                    $"{current}{(string.IsNullOrEmpty(current) ? string.Empty : " AND ")}" +
                    $"([{next.Key}] = '{(next.Value == Values.EmptyViewFilterString ? string.Empty : next.Value)}')");
            this.LocalLogView.RowFilter = rowFilter;
        }

        /// <summary>
        /// Generates view menu items based on row values in the current view.
        /// </summary>
        private void GenerateViewMenu()
        {
            foreach (var menu in this.ViewMenus)
            {
                menu.Children.Clear();
                var filterOptions = this.LocalLog.Rows.OfType<LogRow>()
                    .Where(row => row.Date == this.SelectedLogDate)
                    .Select(row => row[menu.Header].ToString()).Distinct();

                foreach (var option in filterOptions)
                {
                    // Distinguish between an unset filter and a filter set to ''
                    var header = string.IsNullOrEmpty(option) ? Values.EmptyViewFilterString : option;
                    menu.Children.Add(new BoundMenuItem(header)
                    {
                        Command = this.SetFiltersCommand,
                        Parameter = $"{menu.Header},{header}"
                    });
                }

                if (menu.Children.Any())
                {
                    menu.Children.Add(new Separator());
                    menu.Children.Add(new BoundMenuItem("All")
                    {
                        Command = this.SetFiltersCommand,
                        Parameter = menu.Header
                    });
                }
            }
        }

        /// <summary>
        /// Refreshes view elements.
        /// </summary>
        private void RefreshLocalView()
        {
            if (!this.suspendViewRefresh)
            {
                this.suspendViewRefresh = true;

                // Refresh log by date view
                this.GenerateLogViewFilter();

                // Refresh view options menus
                this.GenerateViewMenu();

                // Signal UI elements
                this.RaisePropertyChanged(nameof(this.AreUnassignedCases));
                this.RaisePropertyChanged(nameof(this.ViewFilterDescription));

                this.suspendViewRefresh = false;
            }
        }

        /// <summary>
        /// Sets the status text.
        /// </summary>
        /// <param name="message">The message.</param>
        private void SetStatusText(string message)
        {
            this.StatusTimeStamp = $"[{Tools.GetTimeStamp()}]";

            // Make sure the text changed event fires so the visual effects are triggered
            this.StatusText = string.Empty;
            this.StatusText = message;
        }

        #endregion // View Elements

        #region Property Change Handlers

        /// <summary>
        /// Handles property changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.SelectedAccountRow):
                    var accountFilter = this.SelectedAccountRow != null
                        ? $"([{LogTable.AccountIdColumnName}] = '{this.SelectedAccountRow.AccountId}')"
                        : string.Empty;
                    this.AccountLogView.RowFilter = accountFilter;
                    this.TicketView.RowFilter = accountFilter;
                    if (this.SelectedAccountRow != null)
                    {
                        this.GetRemoteLogByAccount();
                        this.GetRemoteTickets();
                    }

                    break;
                case nameof(this.SelectedLogDate):
                    this.ViewFilters[LogTable.DateColumnName] = $"{this.SelectedLogDate:d}";
                    if (this.IsRemoteConnection)
                    {
                        this.QuickRemoteSync();
                    }
                    else
                    {
                        this.RefreshLocalView();
                    }

                    break;
                case nameof(this.ShowProgress):
                    if (!this.ShowProgress)
                    {
                        this.Progress = 0;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles property changes in log reader.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLogReaderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.logReader.IsConnected):

                    // Make sure we're in the UI thread
                    Application.Current.Dispatcher.Invoke(() => this.SetRemoteStatus(this.logReader.IsConnected));
                    break;
                case nameof(this.logReader.LastException):

                    break;
            }
        }
        #endregion // Property Change Handlers

        #region Row Change Handlers

        /// <summary>
        /// Handles changes in account data rows.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnAccountDetailsRowChanged(object sender, DataRowChangeEventArgs e)
        {
            var row = e.Row as LogRow;
            if (row != null)
            {
                if (!this.suspendRemoteSync && this.isRemoteConnection)
                {
                    // Send account row
                }
                else
                {
                    this.offlineEditAccounts.Add(row.AccountId);
                }
            }
        }

        /// <summary>
        /// Handles changes in log data rows.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnLogRowChanged(object sender, DataRowChangeEventArgs e)
        {
            var row = e.Row as LogRow;
            if (row != null)
            {
                if (!this.suspendRemoteSync && this.isRemoteConnection)
                {
                    // Sync row with remote log
                    var result = this.logReader.UpsertLogByDateAsync(row).Result;
                    if (row.RecordId == LogTable.RecordIdColumnDefaultValue)
                    {
                        // If we just sent a new row, set the record ID
                        this.suspendRemoteSync = true;
                        row.RecordId = result;
                        this.suspendRemoteSync = false;
                    }
                }
                else if (row.RecordId != LogTable.RecordIdColumnDefaultValue)
                {
                    this.offlineEditRecords.Add(row.RecordId);
                }

                this.RefreshLocalView();
            }
        }

        /// <summary>
        /// Handles changes in ticket data rows.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTicketLogRowChanged(object sender, DataRowChangeEventArgs e)
        {
            var row = e.Row as TicketRow;
            if (row != null)
            {
                if (!this.suspendRemoteSync && this.isRemoteConnection)
                {
                    // Send ticket row
                }
                else
                {
                    this.offlineEditTickets.Add(row.TicketId);
                }
            }
        }

        #endregion // Row Change Handlers

        #endregion // Methods
    }
}
