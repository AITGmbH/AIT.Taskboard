using System;
using System.ComponentModel;

namespace AIT.Taskboard.Interface
{
    class StatusItem : IStatusItem
    {
        #region Private Fields

        private string _id;
        private bool _isActive;
        private bool _isProgressing;
        private string _message;
        private bool _isProgressIndeterminate;
        private int _progressPercentComplete;
        private bool _isAutoHide = true;

        #endregion

        #region Implementation of INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        #endregion

        #region Implementation of IStatusItem

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The identification.</value>
        public string Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Id"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Message"));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the progress duration is indeterminate
        /// </summary>
        /// <value>
        /// <c>True</c> if this instance is progress indeterminate; otherwise, <c>false</c>.
        /// </value>
        public bool IsProgressIndeterminate
        {
            get { return _isProgressIndeterminate; }
            set
            {
                if (_isProgressIndeterminate != value)
                {
                    _isProgressIndeterminate = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsProgressIndeterminate"));
                }
            }
        }

        /// <summary>
        /// Gets or sets the completion percentage of all pending operations.
        /// </summary>
        /// <value>The progress percent complete.</value>
        public int ProgressPercentComplete
        {
            get { return _progressPercentComplete; }
            set
            {
                if (_progressPercentComplete != value)
                {
                    _progressPercentComplete = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ProgressPercentComplete"));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the status item is automatically hidden by the status services after a certain time span.
        /// </summary>
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set
            {
                if (value != _isProgressing)
                {
                    _isAutoHide = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsAutoHide"));
                }
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is progressing.
        /// </summary>
        /// <value>
        /// <c>True</c> if this instance is progressing; otherwise, <c>false</c>.
        /// </value>
        public bool IsProgressing
        {
            get { return _isProgressing; }
            set
            {
                if (_isProgressing != value)
                {
                    _isProgressing = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsProgressing"));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value><c>True</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsActive"));
                }
            }
        }

        #endregion
    }
}
