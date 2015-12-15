using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;

namespace AIT.Taskboard.Interface
{
    [Export(typeof(IStatusService))]
    class StatusService : IStatusService
    {
        #region Private fields

        /// <summary>
        /// The currently pending status items
        /// </summary>
        private readonly List<IStatusItem> _statusItems = new List<IStatusItem>();
        private IStatusItem _currentStatusItem;
        private string _currentStatusText;
        private bool _isProgressIndeterminate;
        private bool _isProgressing;
        private int _progressPercentComplete;
        #endregion
        
        [Import]
        ITaskboardService TaskboardService { get; set; }

        #region Implementation of IStatusService

        
        /// <summary>
        /// Gets or sets the current status text.
        /// </summary>
        /// <value>The current status text.</value>
        public string CurrentStatusText
        {
            get { return _currentStatusText; }
            set
            {
                if (_currentStatusText != value)
                {
                    _currentStatusText = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CurrentStatusText"));
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
        /// Enqueues the status item.
        /// </summary>
        /// <param name="item">The item .</param>
        public void EnqueueStatusItem(IStatusItem item)
        {
            _statusItems.Add(item);
            EnsureStatusMessage(true);
            OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Enqueues the status item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns>A new status item instance</returns>
        public IStatusItem EnqueueStatusItem(string itemId)
        {
            IStatusItem item = new StatusItem { Id = itemId, Message = itemId };
            EnqueueStatusItem(item);
            return item;
        }

        /// <summary>
        /// Dequeues the status item.
        /// </summary>
        /// <param name="item">The status item.</param>
        public void DequeueStatusItem(IStatusItem item)
        {
            for (int i = _statusItems.Count - 1; i >= 0; i--)
            {
                IStatusItem statusItem = _statusItems[i];
                if (statusItem == item)
                {
                    item.IsActive = false;
                    _statusItems.Remove(item);
                    EnsureStatusMessage(false);
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Dequeues the status item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        public void DequeueStatusItem(string itemId)
        {
            for (int i = _statusItems.Count - 1; i >= 0; i--)
            {
                IStatusItem statusItem = _statusItems[i];
                if (statusItem.Id == itemId)
                {
                    statusItem.IsActive = false;
                    _statusItems.Remove(statusItem);
                    EnsureStatusMessage(false);
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>
        /// Ensures that the correct status message is set.
        /// </summary>
        private void EnsureStatusMessage(bool respectAutoHide)
        {
            UnwireMessage();
            if ((respectAutoHide) && (_currentStatusItem != null) && (_currentStatusItem.IsAutoHide))
                DequeueStatusItem(_currentStatusItem);
            if (_statusItems.Count > 0)
            {
                IStatusItem currentItem = _statusItems[_statusItems.Count - 1];
                WireMessage(currentItem);
            }
        }

        /// <summary>
        /// Wires the current status message.
        /// </summary>
        /// <param name="currentItem">The current item.</param>
        private void WireMessage(IStatusItem currentItem)
        {
            if (currentItem != null)
            {
                _currentStatusItem = currentItem;
                _currentStatusItem.PropertyChanged += HandleStatusItemPropertyChanged;
                EnsureStatusItemValues(_currentStatusItem);
            }
        }

        /// <summary>
        /// Ensures that the current status item values are applied to the service properties.
        /// </summary>
        /// <param name="item">The current status item.</param>
        private void EnsureStatusItemValues(IStatusItem item)
        {
            if (item != null)
            {
                CurrentStatusText = item.Message;
                IsProgressIndeterminate = item.IsProgressIndeterminate;
                ProgressPercentComplete = item.ProgressPercentComplete;
                IsProgressing = item.IsProgressing;
            }
            else
            {
                CurrentStatusText = string.Empty;
                IsProgressIndeterminate = false;
                ProgressPercentComplete = 0;
                _isProgressing = false;
            }
        }

        /// <summary>
        /// Unwires the current status message.
        /// </summary>
        private void UnwireMessage()
        {
            if (_currentStatusItem != null)
            {
                _currentStatusItem.PropertyChanged -= HandleStatusItemPropertyChanged;
            }
            EnsureStatusItemValues(null);
        }

        /// <summary>
        /// Handles the status item property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void HandleStatusItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnsureStatusItemValues(sender as IStatusItem);
        }

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
    }
}
