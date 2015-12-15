using System.ComponentModel;

namespace AIT.Taskboard.Interface
{
    public interface IStatusService : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the current status text.
        /// </summary>
        /// <value>The current status text.</value>
        string CurrentStatusText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is progressing.
        /// </summary>
        /// <value>
        /// Returns <c>true</c> if this instance is progressing; otherwise, <c>false</c>.
        /// </value>
        bool IsProgressing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is progress indeterminate.
        /// </summary>
        /// <value>
        /// Returns <c>true</c> if this instance is progress indeterminate; otherwise, <c>false</c>.
        /// </value>
        bool IsProgressIndeterminate { get; set; }

        /// <summary>
        /// Gets or sets the progress percent complete.
        /// </summary>
        /// <value>The progress percent complete.</value>
        int ProgressPercentComplete { get; set; }
        
        /// <summary>
        /// Enqueues the status item.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        void EnqueueStatusItem(IStatusItem item);

        /// <summary>
        /// Enqueues the status item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns>A new status item instance</returns>
        IStatusItem EnqueueStatusItem(string itemId);

        /// <summary>
        /// Dequeues the status item.
        /// </summary>
        /// <param name="item">The item to dequeue.</param>
        void DequeueStatusItem(IStatusItem item);

        /// <summary>
        /// Dequeues the status item.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        void DequeueStatusItem(string itemId);
        
    }
}
