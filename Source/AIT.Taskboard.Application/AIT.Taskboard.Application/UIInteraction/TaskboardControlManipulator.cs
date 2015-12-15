using System;

namespace AIT.Taskboard.Application.UIInteraction
{
    /// <summary>
    /// Manipulates the TaskboardControl visual representation.
    /// </summary>
    class TaskboardControlManipulator : IControlManipulation
    {
        /// <summary>
        /// Owning TaskboardControl.
        /// </summary>
        private readonly TaskboardControl _owningControl;

        /// <summary>
        /// Creates a new instance of the TaskboardControlManipulator class.
        /// </summary>
        /// <param name="owningTaskboadControl">TaskboardControl upon which all visual changes are performed.</param>
        public TaskboardControlManipulator(TaskboardControl owningTaskboadControl)
        {
            if(owningTaskboadControl == null) throw new ArgumentException("owningTasboardControl");

            _owningControl = owningTaskboadControl;
        }

        #region IControlManipulation members

        public void ZoomControl(double delta)
        {
            if (delta == 0) return;

            _owningControl.ZoomFactor *= delta;
        }

        #endregion
    }
}
