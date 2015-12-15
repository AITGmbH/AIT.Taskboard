using System.ComponentModel;

namespace AIT.Taskboard.Interface
{
    /// <summary>
    /// Describes a single status item
    /// </summary>
    public interface IStatusItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id of the status item.</value>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is progressing.
        /// </summary>
        /// <value>
        /// Returns <c>true</c> if this instance is progressing; otherwise, <c>false</c>.
        /// </value>
        bool IsProgressing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>Returns <c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        bool IsActive { get; set; }

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
        /// Gets or sets a value indicating whether the status item is automatically hidden by the status services after a certain time span.
        /// </summary>
        bool IsAutoHide { get; set; }
    }
}
