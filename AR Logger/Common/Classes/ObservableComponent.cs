namespace AR_Logger.Common.Classes
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Implements property change notifications.
    /// </summary>
    public abstract class ObservableComponent : INotifyPropertyChanged
    {
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
