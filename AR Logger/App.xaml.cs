namespace AR_Logger
{
    using System.Diagnostics;
    using System.Windows;

    using AR_Logger.Common;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            // Disable notifications for nonfatal binding errors
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

            // Handle all otherwise unhandled errors
            this.DispatcherUnhandledException += Tools.UnhandledExceptionHandler;

            // Set close behavior
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
